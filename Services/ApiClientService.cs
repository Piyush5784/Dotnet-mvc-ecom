using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace VMart.Services
{
    public class ApiClientService
    {
        private readonly HttpClient httpClient;
        private readonly IHttpContextAccessor httpContextAccessor;

        public ApiClientService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            this.httpClient = httpClient;
            this.httpContextAccessor = httpContextAccessor;
        }

        private void AttachJwtToken()
        {
            // Clear any existing authorization header
            httpClient.DefaultRequestHeaders.Authorization = null;

            // Get token from session (consistent with JwtTokenMiddleware)
            var token = httpContextAccessor.HttpContext?.Session.GetString("token");
            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        private string BuildQueryString(object obj)
        {
            if (obj == null)
                return string.Empty;

            var props = obj.GetType().GetProperties()
                .Where(p => p.GetValue(obj) != null)
                .Select(p => $"{Uri.EscapeDataString(p.Name)}={Uri.EscapeDataString(p.GetValue(obj)!.ToString()!)}");

            return string.Join("&", props);
        }

        public async Task<T?> GetAsync<T>(string endpoint, object? payload = null)
        {
            try
            {
                AttachJwtToken();

                if (payload != null)
                {
                    string query = BuildQueryString(payload);
                    endpoint += endpoint.Contains("?") ? "&" + query : "?" + query;
                }




                var response = await httpClient.GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(responseJson);
                }

                return default;
            }
            catch
            {
                return default;
            }
        }

        public async Task<T?> PostAsync<T>(string endpoint, object payload)
        {
            try
            {
                AttachJwtToken();

                HttpContent content;

                // Check if payload contains IFormFile (for file uploads)
                if (payload.GetType().GetProperties().Any(p => p.PropertyType == typeof(IFormFile) ||
                    (p.PropertyType.IsGenericType && p.PropertyType.GetGenericArguments().Contains(typeof(IFormFile)))))
                {
                    // Use multipart/form-data for file uploads
                    var formData = new MultipartFormDataContent();

                    foreach (var prop in payload.GetType().GetProperties())
                    {
                        var value = prop.GetValue(payload);
                        if (value != null)
                        {
                            if (prop.PropertyType == typeof(IFormFile))
                            {
                                var file = (IFormFile)value;
                                var fileContent = new StreamContent(file.OpenReadStream());
                                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                                formData.Add(fileContent, prop.Name, file.FileName);
                            }
                            else if (prop.Name == "Product" && value != null)
                            {
                                // Serialize the Product object and add its properties
                                foreach (var productProp in value.GetType().GetProperties())
                                {
                                    var productValue = productProp.GetValue(value);
                                    if (productValue != null)
                                    {
                                        formData.Add(new StringContent(productValue.ToString() ?? ""), $"Product.{productProp.Name}");
                                    }
                                }
                            }
                            else if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
                            {
                                // Skip complex objects that aren't the Product
                                continue;
                            }
                            else
                            {
                                formData.Add(new StringContent(value?.ToString() ?? ""), prop.Name);
                            }
                        }
                    }
                    content = formData;
                }
                else
                {
                    // Use JSON for regular objects
                    var json = JsonConvert.SerializeObject(payload);
                    content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                var response = await httpClient.PostAsync(endpoint, content);

                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<T>(responseBody);
                }

                return default;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return default;
            }
        }

        public async Task<T?> PutAsync<T>(string endpoint, object payload)
        {
            try
            {
                AttachJwtToken();

                HttpContent content;

                // Check if payload contains IFormFile (for file uploads)
                if (payload.GetType().GetProperties().Any(p => p.PropertyType == typeof(IFormFile) ||
                    (p.PropertyType.IsGenericType && p.PropertyType.GetGenericArguments().Contains(typeof(IFormFile)))))
                {
                    // Use multipart/form-data for file uploads
                    var formData = new MultipartFormDataContent();

                    foreach (var prop in payload.GetType().GetProperties())
                    {
                        var value = prop.GetValue(payload);
                        if (value != null)
                        {
                            if (prop.PropertyType == typeof(IFormFile))
                            {
                                var file = (IFormFile)value;
                                var fileContent = new StreamContent(file.OpenReadStream());
                                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                                formData.Add(fileContent, prop.Name, file.FileName);
                            }
                            else if (prop.Name == "Product" && value != null)
                            {
                                // Serialize the Product object and add its properties
                                foreach (var productProp in value.GetType().GetProperties())
                                {
                                    var productValue = productProp.GetValue(value);
                                    if (productValue != null)
                                    {
                                        formData.Add(new StringContent(productValue.ToString() ?? ""), $"Product.{productProp.Name}");
                                    }
                                }
                            }
                            else if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
                            {
                                // Skip complex objects that aren't the Product
                                continue;
                            }
                            else
                            {
                                formData.Add(new StringContent(value?.ToString() ?? ""), prop.Name);
                            }
                        }
                    }
                    content = formData;
                }
                else
                {
                    // Use JSON for regular objects
                    var json = JsonConvert.SerializeObject(payload);
                    content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                var response = await httpClient.PutAsync(endpoint, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<T>(responseBody);
                }

                return default;
            }
            catch
            {
                return default;
            }
        }

        public async Task<T?> DeleteAsync<T>(string endpoint)
        {
            try
            {
                AttachJwtToken();

                var response = await httpClient.DeleteAsync(endpoint);
                var responseBody = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<T>(responseBody);
                }

                return default;
            }
            catch
            {
                return default;
            }
        }
    }
}
