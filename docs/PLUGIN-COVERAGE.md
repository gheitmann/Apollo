# Optional plugin coverage

The original Apollo listing names 29 integration categories, including two core
features and three instances of MultiShortcut grouped as one category. Skin
definitions do not establish current binary or online service compatibility.

| Integration | Local preparation status |
| --- | --- |
| Moving Pictures | Existing 1.9.7.249 and Backdrop synthetic catalogue retained |
| MP-TVSeries | Existing 4.5.1.764 and synthetic catalogue retained |
| OnlineVideos | Existing 2.9.2.0 retained; providers not certified |
| Trakt | 6.0.7.474 payload installed locally; no account linked |
| TVWishList | Reviewed 1.4.5.0 client DLL is Required32Bit; not installed in local x64 client |
| Pictures | Core feature; existing demo shares retained |
| Extensions | 1.5.0.43 payload installed locally |
| IMDb+ | 2.1.0.581 payload installed locally; no importer run |
| LogoManager | 1.0.5.63 installed locally; incomplete upstream optional Dark Glass design omitted |
| Radio | Core feature; no live service/tuner claim |
| SubCentral | AnyCPU 1.2.7.0 installed locally; provider/language configuration pending |
| WorldWeather Lite | SharpDX 1.3.4.0 local x64 package installed locally; no weather location/API key added |
| MusicInfo Handler | Historical integration; current compatible binary not established |
| InfoService | 1.9.0.81 installed locally; no personal feeds configured |
| BingImage | Historical integration; current provider support not established |
| MyLyrics | 2.2.1.288 from official source archive installed; static API check passes; providers unverified |
| BrowseTheWeb | 1.5.0.1 found in official source archive; Required32Bit Gecko dependency blocks local x64 use |
| Rockstar | Historical integration; current service compatibility not established |
| Airplayer | Official package DLL is Required32Bit; not installed in local x64 client |
| Showtime | Current provider compatibility not established |
| Fritz!Box manager | Hardware integration not configured |
| RadioTime | Current service compatibility not established |
| WorldNews | Current provider compatibility not established |
| mvCentral | 1.0.8.453 installed locally; static API check passes; full import/UI unverified |
| MultiShortcut x3 | External application targets not configured |
| WorldWeather | Alternative to Lite; current compatible binary not established |
| HTPCInfo | TV-server/sensor dependencies not configured |
| MyEmulators2 | External emulators and game media not configured |
| MPSteam | External Steam integration not configured |

Additional helpers: existing Fanart Handler 4.0.5.442 and Latest Media Handler
2.5.0.100 are retained. Global Music Search 2.1.0.12 is installed locally separately.
The first eight newly installed main plugin assemblies are IL-only AnyCPU. That check
does not prove API/dependency or service compatibility. No third-party DLL is
shipped inside the Apollo skin package.

Local deployment completed with destination checksum checks. Individual
services and MPE extension registration have not been certified. Remaining
integrations have not been certified unavailable.

Sources: [Apollo listing](https://www.team-mediaportal.com/extensions/widescreen/apollo),
[maintained plugin repositories](https://github.com/Mediaportal-Plugin-Team),
[SubCentral](https://www.team-mediaportal.com/extensions/movies-videos/subcentral),
[WorldWeather Lite SharpDX](https://www.team-mediaportal.com/extensions/weather/world-weather-lite-sharpdx).
