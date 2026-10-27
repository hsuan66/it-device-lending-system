/**
 * 設備狀態代碼，對應後端 EquipmentStatus；借用中、逾期的代碼與借用紀錄狀態相同
 */
export const EquipmentStatus = {
  Borrowed: 1, // 借用中
  Overdue: 2, // 逾期
  Available: 3, // 可借用
} as const

/**
 * 設備狀態代碼的型別：1 | 2 | 3
 */
export type EquipmentStatus = (typeof EquipmentStatus)[keyof typeof EquipmentStatus]

/**
 * 設備狀態在畫面上顯示的中文名稱
 */
export const equipmentStatusLabels: Record<EquipmentStatus, string> = {
  1: '借用中',
  2: '逾期',
  3: '可借用',
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
  /** 是否已刪除；只有查詢時帶 includeArchived 才可能是 true */
  isArchive: boolean
}

/**
 * 查詢設備清單 API 的篩選條件，不傳代表不篩選
 */
export interface EquipmentQuery {
  typeId?: number
  status?: EquipmentStatus
  /** 是否一併列出已刪除的設備，不傳代表不列出 */
  includeArchived?: boolean
}

/**
 * 新增設備、修改設備 API 送出的資料
 */
export interface EquipmentForm {
  assetCode: string
  name: string
  equipmentTypeId: number
}
