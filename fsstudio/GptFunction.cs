using funcscript.core;
using Newtonsoft.Json;
using OpenAI;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Caching;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Threading.Tasks;

namespace fsstudio
{
    public class SimpleCache
    {
        private Dictionary<string, string> _cache = new Dictionary<string, string>();
        private static string CacheFile = "Cache.json";

        public SimpleCache()
        {
            Load();
        }

        public void Add(string key, string value)
        {
            _cache[key] = value;
            Save();
        }

        public bool Contains(string key)
        {
            return _cache.ContainsKey(key);
        }

        public string Get(string key)
        {
            return _cache[key];
        }

        private void Save()
        {
            File.WriteAllText(CacheFile, JsonConvert.SerializeObject(_cache));
        }

        private void Load()
        {
            if (File.Exists(CacheFile))
            {
                var json = File.ReadAllText(CacheFile);
                _cache = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            }
        }
    }
    internal class GptFunction : funcscript.core.IFsFunction
    {
        private static string CacheFile = "Cache.json";
        public static SimpleCache cache = new SimpleCache();
        public int MaxParsCount => 1;

        public CallType CallType => CallType.Prefix;

        public string Symbol => "gpt";

        public int Precidence => 0;

        public OpenAI.OpenAIClient GetClient()
        {
            if (String.IsNullOrEmpty(Program.OpenAiApiKey))
                throw new InvalidOperationException("OpenAI key not set");
            return new OpenAIClient(Program.OpenAiApiKey);
        }

        public object Evaluate(IFsDataProvider parent, IParameterList pars)
        {
            var prompt = pars.GetParameter(parent, 0) as string;
            if (prompt == null)
                return new InvalidOperationException($"{this.Symbol} - {ParName(0)} is required");

            // Check if response is in cache
            if (cache.Contains(prompt))
            {
                return cache.Get(prompt);
            }

            // If not in cache, get response and add it to cache
            var request = new ChatRequest(new[] { new OpenAI.Chat.Message(Role.User, prompt) }, "gpt-3.5-turbo");
            var response = GetClient().ChatEndpoint.GetCompletionAsync(request);
            Console.Write($"Gpt\nPrompt:\n{prompt}\nWaiting...");
            try
            {
                response.Wait();
                var result = response.Result.Choices[0].Message.Content;
                Console.Write($"Result:\n{result}");
                // Cache the result with an expiry of 1 hour
                cache.Add(prompt, result);
                return result;
            }
            catch(Exception ex) 
            {
                Console.Write($"Error\n{ex.Message}");
                throw;
            }
        }


        public string ParName(int index)
        {
            switch(index)
            {
                case 0:
                    return "Prompt";
                default:
                    return null;
            }
        }
    }
}
