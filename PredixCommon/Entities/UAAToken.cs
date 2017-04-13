using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PredixCommon.Entities
{
    public class UAAToken
    {
        //{
        //  "access_token": "eyJhbGciOiJSUzI1NiIsImtpZCI6ImxlZ2FjeS10b2tlbi1rZXkiLCJ0eXAiOiJKV1QifQ.eyJqdGkiOiIwNmZiMjBiMTgyZjM0YWY5YjcxMzQwOTRkYTYyMGYzNSIsInN1YiI6Im1laC1kcnktcnVuX3N2b3BzIiwic2NvcGUiOlsicHJlZGl4LWFzc2V0LnpvbmVzLmJiYTYxYjdkLTE5NTItNDVhZC1hN2IwLTkyZGViM2Q4YmNlNi51c2VyIiwiZmlsZXN0b3JlLnRlbmFudC4yNTVhZDZiMy0zYWNmLTRlZmMtOGFkYy05MWViOTJjY2E3NTYuaW5nZXN0IiwidGltZXNlcmllcy56b25lcy40ZDNkMjk5YS0zYTNhLTRlMWYtOTVkMy0zYWFmN2UyZDAzZjIudXNlciIsInRpbWVzZXJpZXMuem9uZXMuNGQzZDI5OWEtM2EzYS00ZTFmLTk1ZDMtM2FhZjdlMmQwM2YyLmluZ2VzdCIsImJtLmNsaWVudC5zY29wZSIsInN0dWYucmVhZCIsImFuYWx5dGljcy56b25lcy5kOWE0ZDVkMy00YTJiLTQwYTItOGRkOS1iNzJlYjNmMWZjNGQudXNlciIsInN0dWYuY2VlNWI0NWYtZDMwOS00ODk4LWE1YTItOTEyYzMwMjk2NTc4LnpvbmUiLCJ0aW1lc2VyaWVzLnpvbmVzLjRkM2QyOTlhLTNhM2EtNGUxZi05NWQzLTNhYWY3ZTJkMDNmMi5xdWVyeSJdLCJjbGllbnRfaWQiOiJtZWgtZHJ5LXJ1bl9zdm9wcyIsImNpZCI6Im1laC1kcnktcnVuX3N2b3BzIiwiYXpwIjoibWVoLWRyeS1ydW5fc3ZvcHMiLCJncmFudF90eXBlIjoiY2xpZW50X2NyZWRlbnRpYWxzIiwicmV2X3NpZyI6ImIwOWYwMzYwIiwiaWF0IjoxNDc5Mzc4MTY4LCJleHAiOjE0Nzk0MjEzNjgsImlzcyI6Imh0dHBzOi8vMWVmNTgzYjAtYzhlZi00NDZiLWE4NDYtYjliNTAzZjg5YzI1LnByZWRpeC11YWEucnVuLmFzdi1wci5pY2UucHJlZGl4LmlvL29hdXRoL3Rva2VuIiwiemlkIjoiMWVmNTgzYjAtYzhlZi00NDZiLWE4NDYtYjliNTAzZjg5YzI1IiwiYXVkIjpbImFuYWx5dGljcy56b25lcy5kOWE0ZDVkMy00YTJiLTQwYTItOGRkOS1iNzJlYjNmMWZjNGQiLCJzdHVmIiwic3R1Zi5jZWU1YjQ1Zi1kMzA5LTQ4OTgtYTVhMi05MTJjMzAyOTY1NzgiLCJ0aW1lc2VyaWVzLnpvbmVzLjRkM2QyOTlhLTNhM2EtNGUxZi05NWQzLTNhYWY3ZTJkMDNmMiIsImJtLmNsaWVudCIsImZpbGVzdG9yZS50ZW5hbnQuMjU1YWQ2YjMtM2FjZi00ZWZjLThhZGMtOTFlYjkyY2NhNzU2IiwibWVoLWRyeS1ydW5fc3ZvcHMiLCJwcmVkaXgtYXNzZXQuem9uZXMuYmJhNjFiN2QtMTk1Mi00NWFkLWE3YjAtOTJkZWIzZDhiY2U2Il19.hF9AFcFEgfIC4Hf7KQxkkx9D_-czLnOeccZcgG4tIJ5GXa8OvqwuJPaKdPpfE3dWKaRbtsdKRjgmqPl5qo5WvDfUgiH4AMrsxP7_0kwSbhnpklmA2QcWkwACkNn4W-xFfBYzwCgVlv5akdNIDzwBH4OHpySqfMEh1fJUyex5vIaUPR6rnpRX6IIbtQEfByIQqMfCM0nCY7mj4SpNBcx9UttPuH4-rsdOuyVwuiAZEWPHy3vsxlsBYLvv8eaj3lvLTeELArz1eBN0LDxeP7lA0Vk5ST6Maf0v78uw6qA4xJ97hs0NeDy8nZGARPeoZCpH0IfUdDMpRE9lx2CrjcN9ng",
        //  "token_type": "bearer",
        //  "expires_in": 43199,
        //  "scope": "predix-asset.zones.bba61b7d-1952-45ad-a7b0-92deb3d8bce6.user filestore.tenant.255ad6b3-3acf-4efc-8adc-91eb92cca756.ingest timeseries.zones.4d3d299a-3a3a-4e1f-95d3-3aaf7e2d03f2.user timeseries.zones.4d3d299a-3a3a-4e1f-95d3-3aaf7e2d03f2.ingest bm.client.scope stuf.read analytics.zones.d9a4d5d3-4a2b-40a2-8dd9-b72eb3f1fc4d.user stuf.cee5b45f-d309-4898-a5a2-912c30296578.zone timeseries.zones.4d3d299a-3a3a-4e1f-95d3-3aaf7e2d03f2.query",
        //  "jti": "06fb20b182f34af9b7134094da620f35"
        //}

        internal class JSONUAAToken
        {
            public JSONUAAToken()
            {
                access_token = null;
                token_type = null;
                expires_in = null;
                scope = null;
                jti = null;
            }

            public string access_token;
            public string token_type;
            public string expires_in;
            public string scope;
            public string jti;
        }

        public string AccessToken { get; set; }

        public string TokenType { get; set; }

        public long ExpiresIn { get; set; }

        public string[] Scope { get; set; }

        public string jti { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="data"></param>
        public UAAToken(System.IO.StreamReader data)
        {
            var readString = data.ReadToEnd();

            var jsonObj = JsonConvert.DeserializeObject<JSONUAAToken>(readString);

            this.AccessToken = jsonObj.access_token;
            this.TokenType = jsonObj.token_type;

            long expireInParsedValue;
            bool expireInParsed = long.TryParse(jsonObj.expires_in, out expireInParsedValue);

            if (expireInParsed)
            {
                this.ExpiresIn = expireInParsedValue;
            }

            if (!string.IsNullOrEmpty(jsonObj.scope))
            {
                this.Scope = jsonObj.scope.Split(' ');
            }

            this.jti = jsonObj.jti;
        }
    }
}
