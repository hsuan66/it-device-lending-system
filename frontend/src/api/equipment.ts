import { http, toQueryString } from './http'
import type { Equipment, EquipmentForm, EquipmentQuery } from '@/types/equipment'

/**
 * 查詢設備清單，不含已刪除的設備
 * @param query 篩選條件（類型、狀態），不傳代表不篩選
 * @returns 設備清單，依 id 排序
 */
export function getEquipmentList(query: EquipmentQuery = {}) {
  return http<Equipment[]>(`/Equipment${toQueryString(query)}`)
}

/**
 * 新增設備
 * @param form 財產編號、名稱、類型
 * @returns 沒有內容；財產編號重複時丟出 409、類型不存在時丟出 404 的 ApiError
 */
export function createEquipment(form: EquipmentForm) {
  return http<void>('/Equipment', { method: 'POST', body: JSON.stringify(form) })
}

/**
 * 修改設備
 * @param id 設備 ID
 * @param form 財產編號、名稱、類型
 * @returns 沒有內容；設備不存在或已刪除時丟出 404、財產編號重複時丟出 409 的 ApiError
 */
export function updateEquipment(id: number, form: EquipmentForm) {
  return http<void>(`/Equipment/${id}`, { method: 'PUT', body: JSON.stringify(form) })
}

/**
 * 刪除設備（軟刪除）
 * @param id 設備 ID
 * @returns 沒有內容；設備借用中時丟出 409 的 ApiError
 */
export function deleteEquipment(id: number) {
  return http<void>(`/Equipment/${id}`, { method: 'DELETE' })
}
