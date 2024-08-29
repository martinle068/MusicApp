## Introduction

Welcome to the MusicApp documentation. It will provide a brief introduction to the project's architecture, main ideas and implementation details.

---

## Architecture

### Overview

MusicApp uses a WPF platform and follows an MVVM (Model-View-ViewModel) architectural design pattern, which helps separate the project logic, UI, and data management. The architecture is divided into three primary layers:

- **Model**: This layer provides communication between viewmodels and external sources, in our case with various YouTube APIs or direct communication with internet sources. It mainly offers CRUD operations.
- **ViewModel**: A bridge between models and views. Takes care of the executions of commands and data binding, also fetches and preprocesses data before sendig them to the view.
- **View**: The GUI layer of the application, represented by XAML files. The View binds to the ViewModel, displaying the data and allowing user interaction.

### Components

- **MainViewModel**: The main background layer of the application, stores most important objects, sources and data used by other objects in the app. Includes the mini-player for easier control of the song playback.
- **HomeViewModel**: Manages the home screen, including playlists, popular songs, and search functionality. Also takes care of generating of random content on homescreen, specifically songs in the user's playlists or songs from ther user's favourite authors.
- **PlayerViewModel**: Controls the playback of songs, managing the playlist, and providing media control functionalities.
- **SearchViewModel**: Interacts with an API and handles song searches. The result is sent to the player.

---

## Database

### Design

The application uses an SQLite database to manage song-related data. 

### Tables

- **Songs**: This table stores the song IDs and a count of how often the song has been recommended or played. The schema is as follows:

  | Column | Type   | Description                                  |
  | ------ | ------ | -------------------------------------------- |
  | Id     | TEXT   | Primary key, represents the song ID.         |
  | Count  | INTEGER| Represents how many times the song is played.|

### Operations

The `SongDatabase` class handles all interactions with the SQLite database. Here's a brief overview of the key operations:

- **InsertSong**: Adds a new song to the `Songs` table with an initial count of 0. If the song already exists, the operation is ignored.
- **UpdateSongCount**: Increments the count of a song in the `Songs` table by 1.
- **GetSongCount**: Retrieves the current count of a song from the `Songs` table.
- **GetSongsForRecommendation**: Retrieves 5 songs with the lowest count and 5 random songs from the `Songs` table. It also increments the count of each selected song by 1.
- **IncreaseSongCount**: Increments the count of a song by 1, using the `UpdateSongCount` method.

## API Usage

To be able to work with YouTube account data, specifically read and write operations, it is necessary to setup an [OAuth 2.0](https://support.google.com/cloud/answer/6158849?hl=en) protocol for authentication and authorization.

An API key is not sufficient, it only supports read operations.

## Technologies Used

MusicApp utilized these technologies and libraries to deliver a smooth and enjoyable music experience:

- **Google YouTube API**: Used for authentication and to interact with YouTube's internal data structures, mainly for operations on playlists and fetching popular songs.

- **YouTubeExplode**: A powerful library used to download music and retrieve metadata from YouTube videos.

- **YouTube Music API**: This API is useful for the song search functionality. It also offers detailed song information fetching.

- **LibVLC (VLC Media Player)**: A versatile and robust media player framework used for playing audio within the application. VLC is known for its support of a wide range of audio formats and provides reliable and high-quality playback functionality.

- **SQLite**: A lightweight and simple solution for local data storing without a need for a difficult server setup. It is a file operation based data management system, for our case more than enough. 

- **Windows Presentation Foundation (WPF)**: Used for a simple, intuitive and smooth GUI experience. Styling of the aesthetic part of the application is possible using the XAML file format.

- **System.Threading.Tasks**: Used extensively for asynchronous operations, mainly for song fetching. We are able fetch dozens of songs at a time, leading to a faster loading time.