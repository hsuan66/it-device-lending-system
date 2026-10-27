import { http } from './http'
import type { EquipmentType } from '@/types/equipmentType'

/**
 * 取得所有設備類型
 * @returns 設備類型清單，依 id 排序
 */
export function getEquipmentTypes() {
  return http<EquipmentType[]>('/Type/Equipment')
}
