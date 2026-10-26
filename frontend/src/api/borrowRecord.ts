import { http, toQueryString } from './http'
import type { BorrowRecord, BorrowRecordForm, BorrowRecordQuery } from '@/types/borrowRecord'

/**
 * 查詢借用歷史，包含已刪除設備的紀錄
 * @param query 篩選條件（設備、借用人、狀態），不傳代表不篩選
 * @returns 借用紀錄清單，依借出時間由新到舊
 */
export function getBorrowRecordList(query: BorrowRecordQuery = {}) {
  return http<BorrowRecord[]>(`/EquipmentBorrowRecord${toQueryString(query)}`)
}

/**
 * 新增借用紀錄
 * @param form 設備、借用人、預計歸還時間、借出時間（不填代表現在）
 * @returns 沒有內容；設備借用中時丟出 409、時間不合理時丟出 400 的 ApiError
 */
export function createBorrowRecord(form: BorrowRecordForm) {
  return http<void>('/EquipmentBorrowRecord', { method: 'POST', body: JSON.stringify(form) })
}

/**
 * 歸還設備，歸還時間為呼叫當下
 * @param id 借用紀錄 ID
 * @returns 沒有內容；已經歸還過時丟出 409、紀錄不存在時丟出 404 的 ApiError
 */
export function returnEquipment(id: number) {
  return http<void>(`/EquipmentBorrowRecord/${id}/Return`, { method: 'POST' })
}
