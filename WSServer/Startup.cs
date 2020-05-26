#define UseOptions // or NoOptions or UseOptionsAO
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Debug;
using CollabLib;

namespace EchoApp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(builder =>
            {
                builder.AddConsole()
                    .AddDebug()
                    .AddFilter<ConsoleLoggerProvider>(category: null, level: LogLevel.Debug)
                    .AddFilter<DebugLoggerProvider>(category: null, level: LogLevel.Debug);
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var webSocketOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
                ReceiveBufferSize = 4 * 1024
            };

            app.UseWebSockets(webSocketOptions);

            Document doc = new Document();
            doc.AddMap("nodes");
            List<Document> docs = new List<Document>();
            doc.clientId = 0;
            int currentId = 0;
            app.Use(async (context, next) =>
            {
                if (context.Request.Path.ToString().StartsWith("/doc"))
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        int id = currentId++;
                        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        var newDoc = new Document();
                        docs.Add(newDoc);

                        var buffer = new byte[1024 * 4];
                        WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        while (!result.CloseStatus.HasValue)
                        {
                            byte[] update = new ArraySegment<byte>(buffer).Take(result.Count).ToArray();
                            doc.ApplyUpdate(update);
                            for (int i = 0; i < docs.Count; ++i)
                            {
                                if (i != id)
                                {
                                    docs[i].ApplyUpdate(update);
                                    webSocket.SendAsync(new ArraySegment<byte>(update), WebSocketMessageType.Binary, true, CancellationToken.None);
                                }
                            }
                            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        }
                        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                }
                else
                {
                    await next();
                }

            });
            app.UseFileServer();
        }
    }
}