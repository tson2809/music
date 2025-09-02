export interface Genre {
  genreId: number;
  genreName: string;
  description: string | null;
}

// Interface cho yêu cầu tạo mới (CreateGenreRequest)
export interface CreateGenre {
  genreName: string;
  description: string | null;
}

// Interface cho yêu cầu cập nhật (UpdateGenreRequest)
export interface UpdateGenre {
  genreName: string;
  description: string | null;
}