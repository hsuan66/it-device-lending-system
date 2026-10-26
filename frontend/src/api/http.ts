/**
 * API 呼叫失敗時丟出的錯誤，message 可以直接顯示給使用者
 */
export class ApiError extends Error {
  status: number

  /**
   * 建立 API 錯誤
   * @param status HTTP 狀態碼，例如 400、404、409
   * @param message 顯示給使用者的錯誤訊息
   */
  constructor(status: number, message: string) {
    super(message)
    this.name = 'ApiError'
    this.status = status
  }
}

/**
 * 後端回傳的錯誤格式（ProblemDetails）
 */
interface ProblemDetails {
  title?: string
  errors?: Record<string, string[]>
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
    throw new ApiError(res.status, getErrorMessage(body as ProblemDetails | undefined, res.status))
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

/**
 * 從後端的錯誤回應取出要顯示給使用者的訊息
 * @param body 後端回傳的 ProblemDetails
 * @param status HTTP 狀態碼，沒有訊息時用來組出預設訊息
 * @returns 錯誤訊息，例如「設備借用中，無法借出：2」
 */
function getErrorMessage(body: ProblemDetails | undefined, status: number): string {
  // 欄位驗證錯誤（400）：把每個欄位的訊息串起來
  if (body?.errors) {
    return Object.values(body.errors).flat().join('、')
  }

  // 預存程序丟出的錯誤：title 就是中文訊息
  return body?.title ?? `HTTP ${status}`
}
