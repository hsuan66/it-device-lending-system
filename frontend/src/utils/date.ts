/**
 * 時間的顯示格式：年/月/日 時:分，24 小時制
 */
const dateTimeFormat = new Intl.DateTimeFormat('zh-TW', {
  year: 'numeric',
  month: '2-digit',
  day: '2-digit',
  hour: '2-digit',
  minute: '2-digit',
  hour12: false,
})

/**
 * 把後端回傳的 UTC 時間字串，轉成使用者當地時間的顯示文字
 * @param value ISO 格式的時間字串，例如 2026-11-02T10:00:00+00:00；沒有值時傳 null
 * @returns 例如 2026/11/02 18:00（台灣時間）；沒有值時回傳「—」
 */
export function formatDateTime(value: string | null): string {
  if (!value) {
    return '—'
  }
  return dateTimeFormat.format(new Date(value))
}
