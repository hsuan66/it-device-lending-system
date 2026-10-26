import { ApiError } from '@/api/http'

/**
 * 把各種錯誤轉成可以顯示給使用者的中文訊息
 * @param error catch 到的錯誤
 * @returns 給使用者看的錯誤訊息；後端回傳的錯誤直接使用後端的中文訊息
 */
export function toErrorMessage(error: unknown): string {
  // 原始錯誤印在瀏覽器 console，開發者按 F12 可以看到細節；畫面上只顯示給使用者看的訊息
  console.error(error)

  // 後端回傳失敗的狀態碼：使用後端的中文訊息，例如「設備借用中，無法借出：2」
  if (error instanceof ApiError) {
    return error.message
  }

  // http() 設定的 10 秒逾時
  if (error instanceof DOMException && error.name === 'TimeoutError') {
    return '系統回應時間過長，請稍後再試'
  }

  // 伺服器沒有回應或網路中斷時，fetch 會丟出 TypeError
  if (error instanceof TypeError) {
    return '目前無法連線，請檢查網路連線後再試一次'
  }

  return '系統發生錯誤，請稍後再試'
}
