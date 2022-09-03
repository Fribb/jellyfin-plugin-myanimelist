<h1 align="center">Jellyfin MyAnimeList Plugin</h1>

## About

This Plugin adds the metadata provider for [MyAnimeList.net](https://myanimelist.net/)

> :warning: **The Plugin is not finished yet**

TODO:

* Refine image selection so that the "Main" image is the one being used primarely
* order the list of search results based on a "match score". When searching for a title the search results are not in a "good" order, Searching for "One Piece" will return the actual "One Piece" show at 3rd position while the first one is the ONA "One Piece: We Are ONE.". The search results need to be ordered correctly based on a comparison between the title searched for and the name of the anime itself. levenshteinDistance could be used for this.
* Add Cast/Staff to metadata
* Settings:
  * Preferred Title Language: Main, English, Japanese
  * Preferred Cast/Staff Image: Character, VA (Voice Actor/Actress)
  * Preferred Cast/Staff Language: Japanese, English, Italian, German, Spanish, French
* Use the [anime-lists](https://github.com/Fribb/anime-lists) Project to fill `External IDs` if possible
