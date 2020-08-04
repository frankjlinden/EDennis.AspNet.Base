﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EDennis.NetStandard.Base {
    
    
    public static class IServiceCollectionExtensions_NetStandard {

        public static IServiceCollection AddSecureTokenService<TTokenService>(this IServiceCollection services,
            IConfiguration config, string tokenServiceConfigKey = "Security:ClientCredentials")
            where TTokenService : class, ITokenService {

            services.AddSingleton<ITokenService, TTokenService>();

            if (typeof(ClientCredentialsTokenService).IsAssignableFrom(typeof(TTokenService))) {

                var options = new ClientCredentialsOptions();
                var configSection = config.GetSection(tokenServiceConfigKey);
                configSection.Bind(options);

                services.Configure<ClientCredentialsOptions>(opt => {
                    opt.Authority = options.Authority;
                    opt.ClientId = options.ClientId;
                    opt.ClientSecret = options.ClientSecret;
                    opt.Scope = options.Scope;
                });

                if (options.Authority == null)
                    throw new Exception($"Not able to bind Configuration[\"{tokenServiceConfigKey}\"] to ClientCredentialsOptions");

                services.AddHttpClient("ClientCredentialsTokenService", configure =>
                {
                    configure.BaseAddress = new Uri(options.Authority);
                });

                services.AddAuthentication("Bearer")
                    .AddScheme<BearerTokenOptions, BearerTokenHandler>("Bearer", opt => { });
            }

            return services;
        }


        /// <summary>
        /// Configures the TokenService and the HttpClient for all ProxyQueryControllers
        /// and ProxyCrudControllers.
        /// Note: This extension method requires the existence of a top-level configuration
        /// section called "ProxyClients" that binds to a Dictionary<string,ProxyClient>
        /// and whose key is the name of the Proxy Controller, minus the word Controller.
        /// </summary>
        public static IServiceCollection AddProxyClients<TTokenService>(this IServiceCollection services,
            IConfiguration config, string proxyClientsConfigKey = "ProxyClients")
            where TTokenService : class, ITokenService {

            services.AddSingleton<ITokenService, TTokenService>();

            var clients = new ProxyClients();
            config.GetSection(proxyClientsConfigKey).Bind(clients);

            foreach(var client in clients) {
                var clientName = client.Key;
                services.AddHttpClient(clientName, options => {
                    options.BaseAddress = new Uri(client.Value.TargetUrl);
                });
            }

            return services;
        }
    }
}
