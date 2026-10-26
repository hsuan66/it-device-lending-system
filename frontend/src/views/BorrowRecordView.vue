<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { getBorrowRecordList, returnEquipment } from '@/api/borrowRecord'
import { getEquipmentList } from '@/api/equipment'
import {
  BorrowRecordStatus,
  borrowRecordStatusLabels,
  type BorrowRecord,
  type BorrowRecordQuery,
} from '@/types/borrowRecord'
import type { Equipment } from '@/types/equipment'
import { formatDateTime } from '@/utils/date'
import { toErrorMessage } from '@/utils/error'

/**
 * 設備下拉選單的選項，從設備清單取得（不含已刪除的設備）
 */
const equipmentOptions = ref<Equipment[]>([])

/**
 * 借用狀態下拉選單的選項：借用中、逾期、已歸還
 */
const statusOptions = Object.values(BorrowRecordStatus).map((value) => ({
  value,
  label: borrowRecordStatusLabels[value],
}))

/**
 * 借用狀態對應的標籤顏色：借用中藍色、逾期紅色、已歸還灰色
 */
const statusTagTypes: Record<BorrowRecordStatus, 'primary' | 'danger' | 'info'> = {
  1: 'primary',
  2: 'danger',
  3: 'info',
}

/**
 * 篩選條件，沒填代表不篩選
 */
const query = reactive<BorrowRecordQuery>({
  equipmentId: undefined,
  borrowerName: undefined,
  status: undefined,
})

/**
 * 表格顯示的借用紀錄，後端已依借出時間由新到舊排序
 */
const borrowRecordList = ref<BorrowRecord[]>([])

/**
 * 是否正在載入借用紀錄
 */
const loading = ref(false)

/**
 * 每頁顯示的筆數
 */
const pageSize = 10

/**
 * 目前所在的頁碼，從 1 開始
 */
const currentPage = ref(1)

/**
 * 目前這一頁要顯示的借用紀錄：從完整清單切出第 currentPage 頁的 10 筆
 */
const pagedBorrowRecordList = computed(() => {
  const start = (currentPage.value - 1) * pageSize
  return borrowRecordList.value.slice(start, start + pageSize)
})

/**
 * 載入設備清單，給篩選的設備下拉選單使用
 * @returns 沒有回傳值；失敗時顯示錯誤訊息
 */
async function loadEquipmentOptions() {
  try {
    equipmentOptions.value = await getEquipmentList()
  } catch (error) {
    ElMessage.error(toErrorMessage(error))
  }
}

/**
 * 依目前的篩選條件載入借用紀錄
 * @param keepPage 是否停留在目前的頁碼；歸還後重新載入時傳 true
 * @returns 沒有回傳值；失敗時顯示錯誤訊息
 */
async function loadBorrowRecordList(keepPage = false) {
  loading.value = true
  try {
    borrowRecordList.value = await getBorrowRecordList({
      ...query,
      // 只有空白時視為不篩選
      borrowerName: query.borrowerName?.trim() || undefined,
    })

    if (keepPage) {
      // 篩選「借用中」時歸還後該筆會消失，頁碼不能超過最後一頁
      const lastPage = Math.max(1, Math.ceil(borrowRecordList.value.length / pageSize))
      currentPage.value = Math.min(currentPage.value, lastPage)
    } else {
      // 篩選條件可能變了，回到第 1 頁
      currentPage.value = 1
    }
  } catch (error) {
    ElMessage.error(toErrorMessage(error))
  } finally {
    loading.value = false
  }
}

/**
 * 確認後歸還設備，歸還時間為現在
 * @param record 要歸還的借用紀錄
 * @returns 沒有回傳值；按取消時不做事，失敗時顯示錯誤訊息
 */
async function handleReturn(record: BorrowRecord) {
  try {
    await ElMessageBox.confirm(
      `確定要歸還「${record.assetCode} ${record.equipmentName}」嗎？（借用人：${record.borrowerName}）`,
      '歸還設備',
      { type: 'info', confirmButtonText: '歸還', cancelButtonText: '取消' },
    )
  } catch {
    // 按取消或關閉確認框
    return
  }

  try {
    await returnEquipment(record.id)
    ElMessage.success('歸還成功')
    await loadBorrowRecordList(true)
  } catch (error) {
    ElMessage.error(toErrorMessage(error))
  }
}

/**
 * 清除篩選條件，並重新載入全部借用紀錄
 * @returns 沒有回傳值
 */
function resetQuery() {
  query.equipmentId = undefined
  query.borrowerName = undefined
  query.status = undefined
  loadBorrowRecordList()
}

onMounted(() => {
  loadEquipmentOptions()
  loadBorrowRecordList()
})
</script>

<template>
  <el-card shadow="never">
    <template #header>借用紀錄</template>

    <!-- 篩選條件：按 Enter 或「查詢」都會重新載入 -->
    <el-form :model="query" inline class="filter-form" @submit.prevent="loadBorrowRecordList()">
      <el-form-item label="設備">
        <!-- filterable：可以輸入財產編號或名稱快速找到設備 -->
        <el-select
          v-model="query.equipmentId"
          placeholder="全部"
          clearable
          filterable
          class="filter-select filter-select--wide"
        >
          <el-option
            v-for="equipment in equipmentOptions"
            :key="equipment.id"
            :label="`${equipment.assetCode} ${equipment.name}`"
            :value="equipment.id"
          />
        </el-select>
      </el-form-item>

      <el-form-item label="借用人">
        <el-input
          v-model="query.borrowerName"
          placeholder="姓名"
          maxlength="50"
          clearable
          class="filter-select"
        />
      </el-form-item>

      <el-form-item label="狀態">
        <el-select v-model="query.status" placeholder="全部" clearable class="filter-select">
          <el-option
            v-for="option in statusOptions"
            :key="option.value"
            :label="option.label"
            :value="option.value"
          />
        </el-select>
      </el-form-item>

      <el-form-item>
        <el-button type="primary" native-type="submit" :loading="loading">查詢</el-button>
        <el-button @click="resetQuery">重設</el-button>
      </el-form-item>
    </el-form>

    <!-- scrollbar-always-on：螢幕太窄、欄位放不下時，表格底部一直顯示水平捲軸 -->
    <el-table
      v-loading="loading"
      :data="pagedBorrowRecordList"
      border
      stripe
      scrollbar-always-on
      empty-text="沒有符合條件的借用紀錄"
    >
      <el-table-column prop="assetCode" label="財產編號" min-width="130" />
      <el-table-column prop="equipmentName" label="設備名稱" min-width="200" />
      <el-table-column prop="borrowerName" label="借用人" min-width="100" />
      <el-table-column label="借出時間" min-width="150">
        <template #default="{ row }">{{ formatDateTime((row as BorrowRecord).borrowTime) }}</template>
      </el-table-column>
      <el-table-column label="預計歸還" min-width="150">
        <template #default="{ row }">{{ formatDateTime((row as BorrowRecord).expectReturnTime) }}</template>
      </el-table-column>
      <el-table-column label="實際歸還" min-width="150">
        <template #default="{ row }">{{ formatDateTime((row as BorrowRecord).returnTime) }}</template>
      </el-table-column>
      <el-table-column label="狀態" width="100" align="center">
        <template #default="{ row }">
          <el-tag :type="statusTagTypes[(row as BorrowRecord).status]">
            {{ borrowRecordStatusLabels[(row as BorrowRecord).status] }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column label="操作" width="90" align="center" fixed="right">
        <template #default="{ row }">
          <!--
            只有未歸還（借用中、逾期）的紀錄顯示「歸還」按鈕
            已歸還的紀錄佔大多數，不顯示停用的按鈕，避免一整欄灰色按鈕干擾閱讀
          -->
          <el-button
            v-if="(row as BorrowRecord).status !== BorrowRecordStatus.Returned"
            size="small"
            @click="handleReturn(row as BorrowRecord)"
          >
            歸還
          </el-button>
          <span v-else class="text-muted">—</span>
        </template>
      </el-table-column>
    </el-table>

    <!-- 前端分頁：total 是完整清單的筆數，切換頁碼時 pagedBorrowRecordList 會自動更新 -->
    <el-pagination
      v-model:current-page="currentPage"
      :page-size="pageSize"
      :total="borrowRecordList.length"
      layout="total, prev, pager, next"
      background
      class="pagination"
    />
  </el-card>
</template>

<style scoped>
.filter-select {
  width: 160px;
}

/* 設備選項會顯示「財產編號 名稱」，給寬一點 */
.filter-select--wide {
  width: 240px;
}

/* 已歸還的紀錄在操作欄顯示的「—」 */
.text-muted {
  color: var(--el-text-color-placeholder);
}

.pagination {
  margin-top: 16px;
  justify-content: flex-end;
  flex-wrap: wrap;
}

/* 手機版（寬度 575px 以下）：篩選條件每個一行，輸入框佔滿整行；分頁置中 */
@media (max-width: 575px) {
  .filter-form :deep(.el-form-item) {
    width: 100%;
    margin-right: 0;
  }

  .filter-select,
  .filter-select--wide {
    width: 100%;
  }

  .pagination {
    justify-content: center;
  }
}
</style>
