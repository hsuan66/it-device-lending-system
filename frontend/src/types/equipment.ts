/**
 * 設備狀態代碼，對應後端 EquipmentStatus
 */
export const EquipmentStatus = {
  Available: 1, // 可借用
  Borrowed: 2, // 借用中
  Overdue: 3, // 逾期
} as const

/**
 * 設備狀態代碼的型別：1 | 2 | 3
 */
export type EquipmentStatus = (typeof EquipmentStatus)[keyof typeof EquipmentStatus]

/**
 * 設備狀態在畫面上顯示的中文名稱
 */
export const equipmentStatusLabels: Record<EquipmentStatus, string> = {
  1: '可借用',
  2: '借用中',
  3: '逾期',
}

/**
 * 查詢設備清單 API 回傳的設備
 */
export interface Equipment {
  id: number
  assetCode: string
  name: string
  equipmentTypeId: number
  equipmentTypeName: string
  status: EquipmentStatus
}

/**
 * 查詢設備清單 API 的篩選條件，不傳代表不篩選
 */
export interface EquipmentQuery {
  typeId?: number
  status?: EquipmentStatus
}

/**
 * 新增設備、修改設備 API 送出的資料
 */
export interface EquipmentForm {
  assetCode: string
  name: string
  equipmentTypeId: number
}
