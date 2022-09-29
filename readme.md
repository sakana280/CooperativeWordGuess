# About
This is a collaborative version of a word-guessing game. Either everyone wins, or you are all losers. 
Did someone say "Wordle"? I didn't.

# Tech Stack
[Vue3](https://vuejs.org/) + [Materialize](https://materializecss.com/) frontend 
with an [ASP.NET Core 6.0](https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-6.0) backend.
To keep it simple and cheap, the persistence layer is RAM.
This can be deployed to the free (F1) tier of Azure App Service,
though there could be bandwidth charges if your instance gets too popular.

# Why?
Partly this is an app I wanted to make, but the main drive was to learn 
[Vue without a build step](https://dev.to/krowemoh/a-vue3-tutorial-07-vue-components-without-a-build-system-2p4o) 
and (finally) [WebSockets](https://en.wikipedia.org/wiki/WebSocket)/[SignalR](https://en.wikipedia.org/wiki/SignalR).

In particular, this uses [vue3-sfc-loader](https://github.com/FranckFreiburger/vue3-sfc-loader) 
to avoid needing a build step for the Vue3 frontend, 
as I find compiled frontends with Visual Studio are painful to integrate (need to hack csproj files)
and slow to build (there's no incremental build like C#, so F5/run takes too long).
[The debugging experience is not great yet](https://github.com/FranckFreiburger/vue3-sfc-loader/issues/121), 
and I'll probably run into stale cache issues at my next update,
but build-less Vue was simple to setup and overall I'm happy with how it works. I'll probably do it again.

SignalR was super easy to use. But it quietly swallows any errors in your JavaScript RPC handlers, watch out for that.