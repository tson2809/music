export interface Song {
  songId: number;
  songTitle: string;
  artistId: number;
  artistName: string;
  albumId?: number;
  albumTitle?: string;
  genreId?: number;
  genreName?: string;
  audioFileUrl: string;
  coverImageUrl?: string | null;
  durationSeconds: number;
  releaseDate?: string;
  lyrics?: string;
  playCount: number;
  likeCount: number;
  createdAt: string;
}

export interface UploadSongRequest {
  audioFile: File;
  songTitle: string;
  artistId: number;
  albumId?: number;
  genreId?: number;
  releaseDate?: string;
  lyrics?: string;
}

export interface SongResponse {
  songId: number;
  songTitle: string;
  artistId: number;
  artistName: string;
  albumId?: number;
  albumTitle?: string;
  genreId?: number;
  genreName?: string;
  audioFileUrl: string;
  durationSeconds: number;
  releaseDate?: string;
  message: string;
}

export interface SongListResponse {
  songs: Song[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface Artist {
  artistId: number;
  artistName: string;
}

export interface Album {
  albumId: number;
  albumTitle: string;
  artistId: number;
  artistName: string;
}

export interface Genre {
  genreId: number;
  genreName: string;
}


