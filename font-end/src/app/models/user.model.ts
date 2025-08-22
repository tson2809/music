export interface User {
  userId: number;
  username: string;
  email: string;
  fullName?: string;
  dateOfBirth?: Date;
  country?: string;
  profilePictureUrl?: string;
  roleId: number;
  roleName?: string;
  isActive: boolean;
  artistId?: number;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  user: User;
  message: string;
}

export interface AuthState {
  isAuthenticated: boolean;
  user: User | null;
  token: string | null;
}


export interface UserListResponse {
  users: User[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
