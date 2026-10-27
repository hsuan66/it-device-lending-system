import { ApiError } from '@/api/http'
import { errorMessages } from '@/utils/errorMessages'

/**
 * 把各種錯誤轉成可以顯示給使用者的中文訊息
 * @param error catch 到的錯誤
 * @returns 給使用者看的錯誤訊息；後端回傳的錯誤依錯誤代碼轉換，不直接顯示後端的訊息
 */
export function toErrorMessage(error: unknown): string {
  // 原始錯誤印在瀏覽器 console，開發者按 F12 可以看到後端的訊息與細節；畫面上只顯示給使用者看的訊息
  console.error(error)

  if (error instanceof ApiError) {
    // 後端有回傳錯誤代碼，而且是已知的代碼
    const message = error.code !== undefined ? errorMessages[error.code] : undefined
    if (message) {
      return message
    }

    // 沒有代碼或不認得的代碼：400 是輸入有誤，其他（例如 500）一律視為系統錯誤
    if (error.status === 400) {
      return '輸入的資料有誤，請檢查後再試一次'
    }
    return '系統發生錯誤，請稍後再試'
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
