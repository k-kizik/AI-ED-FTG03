import client from './client';
import type { LoginRequest, LoginResult, RegisterRequest } from '../types/api';

export async function login(data: LoginRequest): Promise<LoginResult> {
  const res = await client.post<LoginResult>('/auth/login', data);
  return res.data;
}

export async function register(data: RegisterRequest): Promise<void> {
  await client.post('/auth/register', data);
}
