export interface Todo {
  id: string
  title: string
  description: string | null
  completedAt: string | null
  createdAt: string
}

export interface CreateTodoRequest {
  title: string
  description?: string
}

export interface UpdateTodoRequest {
  title: string
  description?: string
  completedAt?: string | null
}

const API_BASE = '/api/todos'

export const todosApi = {
  async getAll(): Promise<Todo[]> {
    const response = await fetch(API_BASE)
    if (!response.ok) throw new Error('Failed to fetch todos')
    return response.json()
  },

  async getById(id: string): Promise<Todo> {
    const response = await fetch(`${API_BASE}/${id}`)
    if (!response.ok) throw new Error('Failed to fetch todo')
    return response.json()
  },

  async create(data: CreateTodoRequest): Promise<Todo> {
    const response = await fetch(API_BASE, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    })
    if (!response.ok) throw new Error('Failed to create todo')
    return response.json()
  },

  async update(id: string, data: UpdateTodoRequest): Promise<Todo> {
    const response = await fetch(`${API_BASE}/${id}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    })
    if (!response.ok) throw new Error('Failed to update todo')
    return response.json()
  },
}
