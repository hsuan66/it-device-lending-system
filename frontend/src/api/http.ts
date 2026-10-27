/**
 * API 呼叫失敗時丟出的錯誤
 * message 是後端給開發者看的訊息；畫面上要依 code 轉成使用者訊息（見 utils/error.ts）
 */
export class ApiError extends Error {
  status: number
  code?: number

  /**
   * 建立 API 錯誤
   * @param status HTTP 狀態碼，例如 400、404、409
   * @param message 後端給開發者看的錯誤訊息
   * @param code 後端的錯誤代碼，例如 2006；後端沒有回傳時為 undefined
   */
  constructor(status: number, message: string, code?: number) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.code = code
  }
}

/**
 * 後端回傳的錯誤格式，例如 { code: 2006, message: '借用設備失敗：設備借用中（...）' }
 */
interface ApiErrorBody {
  /** 錯誤代碼 */
  code?: number
  /** 給開發者看的訊息 */
  message?: string
}

/**
 * 呼叫後端 API，網址會自動加上 /api 前綴
 * @param path API 路徑，例如 /Equipment
 * @param options fetch 的設定，例如 method、body
 * @returns 後端回傳的資料；沒有內容（201、204）時為 undefined
 * @throws ApiError 後端回傳失敗的狀態碼時
 */
export async function http<T>(path: string, options: RequestInit = {}): Promise<T> {
  const res = await fetch(`/api${path}`, {
    signal: AbortSignal.timeout(10000),
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...options.headers,
    },
  })

  // 201、204 可能沒有內容，先讀成文字，有內容才轉成 JSON
  const text = await res.text()
  const body = text ? parseJson(text) : undefined

  if (!res.ok) {
    // 後端沒開時可能回傳非 JSON 的錯誤頁，body 為 undefined，訊息改用 HTTP 狀態碼
    const error = body as ApiErrorBody | undefined
    throw new ApiError(res.status, error?.message ?? `HTTP ${res.status}`, error?.code)
  }

  return body as T
}

/**
 * 把查詢條件轉成查詢字串，略過沒有值的條件
 * @param params 查詢條件，例如 { typeId: 1, status: undefined }
 * @returns 查詢字串，例如 ?typeId=1；沒有任何條件時為空字串
 */
export function toQueryString(params: object): string {
  const search = new URLSearchParams()
  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined && value !== null && value !== '') {
      search.append(key, String(value))
    }
  }
  const query = search.toString()
  return query ? `?${query}` : ''
}

/**
 * 把文字轉成 JSON，轉換失敗時不丟錯誤
 * @param text 回應的文字內容
 * @returns 轉換後的物件；不是 JSON（例如後端沒開時的錯誤頁）時為 undefined
 */
function parseJson(text: string): unknown {
  try {
    return JSON.parse(text)
  } catch {
    return undefined
  }
}
