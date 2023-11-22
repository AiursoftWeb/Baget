# BaGet

[![MIT licensed](https://img.shields.io/badge/license-MIT-blue.svg)](https://gitlab.aiursoft.cn/aiursoft/baget/-/blob/master/LICENSE)
[![Pipeline stat](https://gitlab.aiursoft.cn/aiursoft/baget/badges/master/pipeline.svg)](https://gitlab.aiursoft.cn/aiursoft/baget/-/pipelines)
[![Test Coverage](https://gitlab.aiursoft.cn/aiursoft/baget/badges/master/coverage.svg)](https://gitlab.aiursoft.cn/aiursoft/baget/-/pipelines)
[![ManHours](https://manhours.aiursoft.cn/gitlab/gitlab.aiursoft.cn/aiursoft/baget)](https://gitlab.aiursoft.cn/aiursoft/baget/-/commits/master?ref_type=heads)
[![Website](https://img.shields.io/website?url=https%3A%2F%2Fnuget.aiursoft.cn%2F)](https://nuget.aiursoft.cn)

A lightweight [NuGet] and [symbol] server.

<p align="center">
  <img width="100%" src="https://user-images.githubusercontent.com/737941/50140219-d8409700-0258-11e9-94c9-dad24d2b48bb.png">
</p>

## Getting Started

1. Install the [.NET SDK]
2. Download and extract [BaGet's latest release]
3. Start the service with `dotnet BaGet.dll`
4. Browse `http://localhost:5000/` in your browser

For more information, please refer to the [documentation].

## Features

* **Cross-platform**: runs on Windows, macOS, and Linux!
* **Cloud native**: supports [Docker], [Azure], [AWS], [Google Cloud], [Alibaba Cloud]
* **Offline support**: [mirror a NuGet server] to speed up builds and enable offline downloads

Stay tuned, more features are planned!

[Build status]: https://img.shields.io/github/actions/workflow/status/loic-sharma/BaGet/.github/workflows/main.yml
[Discord image]: https://img.shields.io/discord/889377258068930591
[Discord link]: https://discord.gg/MWbhpf66mk
[Twitter image]: https://img.shields.io/twitter/follow/bagetapp?label=Follow
[Twitter link]: https://twitter.com/bagetapp

[NuGet]: https://learn.microsoft.com/nuget/what-is-nuget
[symbol]: https://docs.microsoft.com/en-us/windows/desktop/debug/symbol-servers-and-symbol-stores
[.NET SDK]: https://www.microsoft.com/net/download
[Node.js]: https://nodejs.org/

[BaGet's latest release]: https://github.com/loic-sharma/BaGet/releases

[Documentation]: https://loic-sharma.github.io/BaGet/
[Docker]: https://loic-sharma.github.io/BaGet/installation/docker/
[Azure]: https://loic-sharma.github.io/BaGet/installation/azure/
[AWS]: https://loic-sharma.github.io/BaGet/installation/aws/
[Google Cloud]: https://loic-sharma.github.io/BaGet/installation/gcp/
[Alibaba Cloud]: https://loic-sharma.github.io/BaGet/installation/aliyun/

[Mirror a NuGet server]: https://loic-sharma.github.io/BaGet/configuration/#enable-read-through-caching