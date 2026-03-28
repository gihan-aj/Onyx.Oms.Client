using Duende.IdentityModel.OidcClient.Browser;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Shared.Services;

public class SystemBrowser : IBrowser
{
    public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default)
    {
        using var listner = new HttpListener();

        try
        {
            // Start the listener on the Redirect URI port
            var prefix = options.EndUrl;
            if(!prefix.EndsWith("/"))
                prefix = prefix + "/";

            listner.Prefixes.Add(prefix);
            listner.Start();

            // Open the dfault browser
            Process.Start(new ProcessStartInfo
            {
                FileName = options.StartUrl,
                UseShellExecute = true,
            });

            // Wait for the IdP to redirect
            var context = await listner.GetContextAsync();

            // Send a self closing response back to the browser
            var response = context.Response;
            string responseString = @"
                <html>
                    <head><title>Authentication Complete</title></head>
                    <body>
                        <script>
                            // Automatically close this tab to return to the app
                            window.close();
                        </script>
                        <p>Login successful! You can close this tab and return to the application.</p>
                    </body>
                </html>";

            var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.Close();

            return new BrowserResult
            {
                Response = context.Request.Url?.ToString(),
                ResultType = BrowserResultType.Success,
            };
        }
        catch (Exception ex)
        {

            return new BrowserResult { ResultType = BrowserResultType.UnknownError, Error = ex.Message };
        }
        finally
        {
            if (listner.IsListening)
            {
                listner.Stop();
            }
        }

        //try
        //{
        //    var processStartInfo = new ProcessStartInfo
        //    {
        //        FileName = options.StartUrl,
        //        UseShellExecute = true
        //    };

        //    Process.Start(processStartInfo);

        //    // We don't need to wait for the browser here really, the OidcClient loopback listener will handle the response.
        //    // But IBrowser requires returning a result.
        //    // The actual "result" processing happens via the RedirectUri listener which OidcClient manages.
        //    // However, IdentityModel.OidcClient expectation for 'InvokeAsync' is that it *returns* the result 
        //    // after the flow is done IF we were doing a manual loopback, but OidcClient handles loopback 
        //    // if we don't provide a custom implementation of the listener.
        //    // NOTE: IdentityModel.OidcClient 5.x+ has different IBrowser interface than older ones.
        //    // With OidcClient 5+, the IBrowser is just responsible for launching the browser.
        //    // But wait, OidcClient handles the listener? Yes.
        //    // So we just launch the URL.
            
        //    // Wait for the result... OidcClient passes a 'PrepareLoginAsync' context usually?
        //    // Actually, in OidcClient 5, the InvokeAsync is awaited until the flow completes.
        //    // But since we are using the system browser, we can't easily "wait" for it to close or finish 
        //    // without a custom listener. 
        //    // HOWEVER, IdentityModel.OidcClient provides a default loopback listener logic.
        //    // We just need to launch the browser.
        //    // But how does InvokeAsync know when to return?
            
        //    // Ah, usually creating a SystemBrowser involves passing the listener logic or just returning 
        //    // a distinct result type.
            
        //    // Let's look at standard implementations for WinUI/Desktop.
        //    // Standard approach: 
        //    // 1. Open Browser.
        //    // 2. The OidcClient's internal listener picks up the request.
        //    // 3. BUT InvokeAsync signature requires returning BrowserResult.
        //    // If we are using the internal listener, we might be strictly responsible for OPENING the browser, 
        //    // but the orchestration is done by OidcClient.
            
        //    // Actually, looking at IdentityModel.OidcClient documentation:
        //    // The IBrowser.InvokeAsync is called *by* OidcClient. 
        //    // It expects us to open the browser.
        //    // AND return the result *when* the browser interaction is done.
        //    // This suggests we need to signal completion.
        //    // BUT for System Browser with Loopback, OidcClient has a `LoopbackHttpListener`. 
        //    // We usually don't implement IBrowser manually if we use the default... 
        //    // Wait, `OidcClientOptions.Browser` IS required.
            
        //    // The standard `SystemBrowser` implementation typically sets up a listener OR assumes the caller does. 
        //    // In `IdentityModel.OidcClient`, the `LoginAsync` method starts the loopback listener BEFORE calling `Browser.InvokeAsync`.
        //    // Wait, no. `OidcClient` does NOT have a built-in HTTP listener in the core package?
        //    // "OidcClient.d.ts" or similar... 
        //    // Actually it DOES, but typically we need to help it.
            
        //    // Let's implement a simple SystemBrowser that just opens the URL and returns "Success" immediately?
        //    // No, if we return immediately, the `LoginAsync` might continue before the user has logged in?
        //    // No, `LoginAsync` waits for the Authorization Code.
        //    // The `IBrowser` interface is: `Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken)`.
            
        //    // If we use the "Manual" mode (no loopback), we return the URL.
        //    // If we use "Loopback", we need a listener.
            
        //    // Let's assume we use the built-in loopback listener if available?
        //    // Actually, `IdentityModel.OidcClient` separates the concerns. 
        //    // We usually just need to open the browser.
        //    // But `InvokeAsync` returns `BrowserResult`.
            
        //    // Ref: https://github.com/IdentityModel/IdentityModel.OidcClient/blob/main/src/OidcClient/Browser/IBrowser.cs
        //    // It seems we need to coordinate.
            
        //    // BUT, for a simple implementation: 
        //    // We can rely on `DefaultBrowser` from `IdentityModel.OidcClient.Extensions`? 
        //    // We don't have that package.
            
        //    // Let's try this implementation:
        //    // We just open the URL.
        //    // But we can't return logic result. 
        //    // Wait, if we are using loopback, the valid response comes to the listener.
        //    // Who runs the listener?
            
        //    // INCREDIBLY IMPORTANT:
        //    // `IdentityModel.OidcClient` handles the loopback listener automatically if we don't provide one?
        //    // No, strictly keying off `options.Browser`.
            
        //    // Let's check `IdentityModel.OidcClient` version.
        //    // We are using 7.0.0.
            
        //    // The simple `SystemBrowser` implementation for OidcClient usually looks like this:
            
        //    // public class SystemBrowser : IBrowser {
        //    //    public Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken t) {
        //    //        Process.Start(options.StartUrl);
        //    //        return Task.FromResult(new BrowserResult { ResultType = BrowserResultType.Unknown });
        //    //    }
        //    // }
            
        //    // Wait, if we return immediately, `OidcClient` might think we are done.
        //    // UNLESS `OidcClient` is waiting for the loopback listener too.
        //    // The `LoginAsync` flow:
        //    // 1. Prepare login (generate state, etc).
        //    // 2. Start browser (via IBrowser).
        //    // 3. Wait for result?
            
        //    // Actually, usually `IBrowser` implementation *is* the one doing the waiting if it's a "WebAuthenticationBroker".
        //    // If it's a System Browser + Loopback, `IBrowser` usually just opens the URL. 
        //    // BUT `LoginAsync` needs the response.
            
        //    // Correction: `OidcClient` *does not* include a loopback listener by default in the core package? 
        //    // It relies on `browser.InvokeAsync` to return the full result (URL with code).
            
        //    // So WE need to implement the loopback listener inside `SystemBrowser`.
        //    // Ah! That explains the "The IdentityModel.OidcClient library will automatically start a temporary local web listener" claim I made.
        //    // I might have been mistaken about the *core* library doing it automatically without code.
        //    // The *IdentityModel.OidcClient.Desktop* (or similar) extension packages do it.
        //    // Since we only added `IdentityModel.OidcClient`, we likely need to implement the listener in `SystemBrowser`.
            
        //    // Implementation Plan for SystemBrowser with Loopback:
        //    // 1. Create an `HttpListener`.
        //    // 2. Start listening on the RedirectUri port (127.0.0.1:7890).
        //    // 3. Open the browser with `options.StartUrl`.
        //    // 4. Wait for a request to the listener.
        //    // 5. Read the request (this contains the code).
        //    // 6. Return the full URL as `BrowserResult`.
        //    // 7. Write a simple "You can close this tab" response to the browser.
        //    // 8. Stop listener.
            
        //    return await LoopbackHttpListener(options);
        //}
        //catch (Exception ex)
        //{
        //    return new BrowserResult { ResultType = BrowserResultType.UnknownError, Error = ex.Message };
        //}
    }

    private async Task<BrowserResult> LoopbackHttpListener(BrowserOptions options)
    {
        using var listener = new System.Net.HttpListener();
        var prefix = options.EndUrl;
        if (!prefix.EndsWith("/")) prefix += "/";
        listener.Prefixes.Add(prefix);
        
        // Ensure options.EndUrl ends with / check?
        // IdentityModel.OidcClient usually ensures EndUrl is the RedirectUri.
        
        listener.Start();

        // Check if we need to open the browser
        // options.StartUrl contains the authorization request URL
        var processStartInfo = new ProcessStartInfo
        {
            FileName = options.StartUrl,
            UseShellExecute = true
        };
        Process.Start(processStartInfo);

        // Wait for the callback
        var context = await listener.GetContextAsync();

        // Handle the request
        var formData = context.Request.Url?.ToString();

        // Send a response to the browser
        var response = context.Response;
        string responseString = "<html><body>You can now return to the application.</body></html>";
        var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
        response.ContentLength64 = buffer.Length;
        var responseOutput = response.OutputStream;
        await responseOutput.WriteAsync(buffer, 0, buffer.Length);
        responseOutput.Close();

        return new BrowserResult
        {
            Response = formData,
            ResultType = BrowserResultType.Success
        };
    }
}
