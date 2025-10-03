export interface Album {
  albumId: number;
  albumTitle: string;
  artistId: number;
  artistName: string;
  releaseDate?: string | null;
  albumType: string;
  coverImageUrl?: string | null;
  totalTracks: number;
  durationSeconds: number;
  createdAt: string;
}

export interface AlbumSong {
  songId: number;
  songTitle: string;
  durationSeconds: number;
  genreName?: string | null;
  audioFileUrl: string;
  coverImageUrl?: string | null;
  approvalStatus: string;
}

export interface AlbumDetail extends Album {
  songs: AlbumSong[];
}

export interface AlbumListResponse {
  albums: Album[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface CreateAlbumRequest {
  albumTitle: string;
  artistId: number;
  releaseDate?: string;
  albumType?: string;
  coverImageFile?: File;
}

export interface UpdateAlbumRequest {
  albumTitle: string;
  releaseDate?: string;
  albumType?: string;
  coverImageFile?: File;
}

