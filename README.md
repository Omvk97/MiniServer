The package is available on nuget.org as MiniServer

Here is a simple usage example:

```
using System;
using MiniServer;

namespace GitHubDemoTest
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            MinServer test = new MinServer();

            test.Get("Test", async (req, res) =>
            {
                await res.SendJSONAsync(new { Test = "Test" });
            });

            await test.Start();
        }
    }
}

```

It is meant to be almost like Express (But a minimal version). If some methods you use from Express are needed. Please contact me or create a pull request!
