# melomania

### A CLI tool for managing your Google Drive music collection

> Note: The tool is still under development. If you want to try it out right now, you'll have to generate yourself some [Google Console](https://console.cloud.google.com) credentials and build it yourself. Just download your `credentials.json` file and place it at the root of your project.

### Usage

#### Setup

``` sh
$ melomania setup
...
Enter your base music folder: 
```
Downloads the necessary tools, prompts you to authenticate with Google through OAuth and set your base music collection folder.

#### Download tools

``` sh
$ melomania download-tools
Downloading 'youtube-dl'...
Successfully downloaded 'youtube-dl'!
Downloading 'ffmpeg'...
Successfully downloaded 'ffmpeg'!
...
```
Re-downloads the tools if for some reason they were corrupted.

#### List

``` sh
$ melomania list <collection-folder-path> ('.' for root)
```

Lists the contents of the given path. 

``` sh
$ melomania list .
Fetching collection contents...

Tracks:
amazing-song.mp3
an-even-better-song.mp3

Folders:
Album One
Album Two
```

#### Upload

For all uploads, the `<collection-folder-path>` argument is a path relative to your base collection folder.

E.g. if you've set your base collection folder to `Music`, passing in `Album One` as a collection folder path will upload the track into `Music\Album One`.

##### From Url

``` sh
melomania upload url <url> <collection-folder-path> ('.' for root) <[optional-custom-filename]>
```

Extracts the .mp3 from a video url (e.g. Youtube) and uploads it to a path inside your music collection. Takes in an optional custom file name. If none is specified, the video title is going to be used.

``` sh
melomania upload url https://www.youtube.com/watch?v=oHg5SJYRHA0 . "Never gonna give you up"
Extracting 'RickRoll'D.mp3'...
'RickRoll'D.mp3' progress: 100%
Successfully extracted 'RickRoll'D.mp3'!
Uploading 'Never gonna give you up.mp3' into 'Music'...
'Never gonna give you up.mp3' upload progress: 7%
...
'Never gonna give you up.mp3' upload progress: 93%
Successfully uploaded 'Never gonna give you up.mp3' into 'Music'!
```

##### From Path

``` sh
melomania upload path <physical-path> <collection-folder-path> ('.' for root)
```

Uploads a track from a physical path to a path inside your music collection.

``` sh
melomania upload path "./Improvisation.mp3" "Improvisations"
Uploading 'Improvisation.mp3' into 'Music\Improvisations'...
'Improvisation.mp3' upload progress: 2%
...
'Improvisation.mp3' upload progress: 100%
Successfully uploaded 'Improvisation.mp3' into 'Music\Improvisations'
```

### Installation

... in progress
