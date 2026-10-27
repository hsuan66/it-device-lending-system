/**
 * 判斷篩選條件有沒有填；下拉選單按清除後，值可能是 undefined、null 或空字串
 * @param value 篩選條件的值
 * @returns true 代表有填（只有空白的字串視為沒填）
 */
export function hasFilterValue(value: unknown): boolean {
  if (typeof value === 'string') {
    return value.trim() !== ''
  }
  return value !== undefined && value !== null
}
