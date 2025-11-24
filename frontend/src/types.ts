import { AgentConfigurationDto } from './api/generated/api';

/**
 * The generated API client uses `void` for return types due to missing response schemas.
 * These types bridge the gap for runtime casting.
 */

export interface ConversationDto {
  id: string;
  title?: string;
  createdAt?: string;
  updatedAt?: string;
  configuration?: AgentConfigurationDto;
}

export interface MessageDto {
  id: string;
  conversationId: string;
  content: string;
  role: 'user' | 'assistant' | 'system';
  createdAt: string;
  attachments?: any[];
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

// Helper to safely cast the void API responses
export const castTo = <T>(data: any): T => data as unknown as T;