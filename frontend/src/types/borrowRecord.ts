/**
 * 借用紀錄狀態代碼，對應後端 BorrowRecordStatus
 */
export const BorrowRecordStatus = {
  Borrowed: 1, // 借用中
  Overdue: 2, // 逾期
  Returned: 3, // 已歸還
} as const

/**
 * 借用紀錄狀態代碼的型別：1 | 2 | 3
 */
export type BorrowRecordStatus = (typeof BorrowRecordStatus)[keyof typeof BorrowRecordStatus]

/**
 * 借用紀錄狀態在畫面上顯示的中文名稱
 */
export const borrowRecordStatusLabels: Record<BorrowRecordStatus, string> = {
  1: '借用中',
  2: '逾期',
  3: '已歸還',
}

/**
 * 查詢借用歷史 API 回傳的借用紀錄，時間是 UTC 的 ISO 字串
 */
export interface BorrowRecord {
  id: number
  equipmentId: number
  assetCode: string
  equipmentName: string
  equipmentTypeName: string
  borrowerName: string
  borrowTime: string
  expectReturnTime: string
  returnTime: string | null
  status: BorrowRecordStatus
}

/**
 * 查詢借用歷史 API 的篩選條件，不傳代表不篩選
 */
export interface BorrowRecordQuery {
  equipmentId?: number
  borrowerName?: string
  status?: BorrowRecordStatus
}

/**
 * 新增借用紀錄 API 送出的資料
 */
export interface BorrowRecordForm {
  equipmentId: number
  borrowerName: string
  expectReturnTime: string
  borrowTime?: string
}
