export async function http<T>(path: string, options: RequestInit = {}): Promise<T> {
  const res = await fetch(`/api${path}`, {
    signal: AbortSignal.timeout(10000),
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...options.headers,
    },
  })

  if (!res.ok) {
    throw new Error(`HTTP ${res.status}`)
  }

  if (res.status === 204) {
    return undefined as T
  }

  return res.json() as Promise<T>
}