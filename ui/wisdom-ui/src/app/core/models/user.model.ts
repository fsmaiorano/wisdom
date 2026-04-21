export enum UserRoleEnum {
  Admin = 0,
  Teacher = 1,
  Customer = 2,
}

export const USER_ROLE_LABELS: Record<UserRoleEnum, string> = {
  [UserRoleEnum.Admin]: 'Admin',
  [UserRoleEnum.Teacher]: 'Teacher',
  [UserRoleEnum.Customer]: 'Student',
};

export interface User {
  id: string;
  keycloakId: string;
  email: string;
  name: string;
  role: UserRoleEnum;
}

export interface AuthResponse {
  user: User;
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
}

export interface SignInRequest {
  email: string;
  password: string;
}

export interface SignUpRequest {
  email: string;
  name: string;
  password: string;
  role: UserRoleEnum;
}
