<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { ElMessage } from 'element-plus'
import { getEquipmentList } from '@/api/equipment'
import { getEquipmentTypes } from '@/api/equipmentType'
import {
  EquipmentStatus,
  equipmentStatusLabels,
  type Equipment,
  type EquipmentQuery,
} from '@/types/equipment'
import type { EquipmentType } from '@/types/equipmentType'
import { toErrorMessage } from '@/utils/error'

/**
 * 設備類型下拉選單的選項，從後端取得
 */
const equipmentTypes = ref<EquipmentType[]>([])

/**
 * 設備狀態下拉選單的選項：可借用、借用中、逾期
 */
const statusOptions = Object.values(EquipmentStatus).map((value) => ({
  value,
  label: equipmentStatusLabels[value],
}))

/**
 * 設備狀態對應的標籤顏色：可借用綠色、借用中藍色、逾期紅色
 */
const statusTagTypes: Record<EquipmentStatus, 'success' | 'primary' | 'danger'> = {
  1: 'success',
  2: 'primary',
  3: 'danger',
}

/**
 * 篩選條件，沒選代表不篩選
 */
const query = reactive<EquipmentQuery>({
  typeId: undefined,
  status: undefined,
})

/**
 * 表格顯示的設備清單
 */
const equipmentList = ref<Equipment[]>([])

/**
 * 是否正在載入設備清單
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
 * 目前這一頁要顯示的設備：從完整清單切出第 currentPage 頁的 10 筆
 */
const pagedEquipmentList = computed(() => {
  const start = (currentPage.value - 1) * pageSize
  return equipmentList.value.slice(start, start + pageSize)
})

/**
 * 載入設備類型，給篩選的下拉選單使用
 * @returns 沒有回傳值；失敗時顯示錯誤訊息
 */
async function loadEquipmentTypes() {
  try {
    equipmentTypes.value = await getEquipmentTypes()
  } catch (error) {
    ElMessage.error(toErrorMessage(error))
  }
}

/**
 * 依目前的篩選條件載入設備清單
 * @returns 沒有回傳值；失敗時顯示錯誤訊息
 */
async function loadEquipmentList() {
  loading.value = true
  try {
    equipmentList.value = await getEquipmentList(query)
    // 篩選條件可能變了，回到第 1 頁
    currentPage.value = 1
  } catch (error) {
    ElMessage.error(toErrorMessage(error))
  } finally {
    loading.value = false
  }
}

/**
 * 清除篩選條件，並重新載入全部設備
 * @returns 沒有回傳值
 */
function resetQuery() {
  query.typeId = undefined
  query.status = undefined
  loadEquipmentList()
}

onMounted(() => {
  loadEquipmentTypes()
  loadEquipmentList()
})
</script>

<template>
  <el-card shadow="never">
    <template #header>設備管理</template>

    <!-- 篩選條件：按 Enter 或「查詢」都會重新載入 -->
    <el-form :model="query" inline class="filter-form" @submit.prevent="loadEquipmentList">
      <el-form-item label="設備類型">
        <el-select v-model="query.typeId" placeholder="全部" clearable class="filter-select">
          <el-option
            v-for="equipmentType in equipmentTypes"
            :key="equipmentType.id"
            :label="equipmentType.name"
            :value="equipmentType.id"
          />
        </el-select>
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

    <!-- scrollbar-always-on：螢幕太窄、欄位放不下時，表格底部一直顯示水平捲軸（預設只有滑鼠移上去才顯示） -->
    <el-table
      v-loading="loading"
      :data="pagedEquipmentList"
      border
      stripe
      scrollbar-always-on
      empty-text="沒有符合條件的設備"
    >
      <el-table-column prop="assetCode" label="財產編號" min-width="140" />
      <el-table-column prop="name" label="設備名稱" min-width="220" />
      <el-table-column prop="equipmentTypeName" label="類型" min-width="120" />
      <el-table-column label="狀態" width="100" align="center">
        <template #default="{ row }">
          <el-tag :type="statusTagTypes[(row as Equipment).status]">
            {{ equipmentStatusLabels[(row as Equipment).status] }}
          </el-tag>
        </template>
      </el-table-column>
    </el-table>

    <!-- 前端分頁：total 是完整清單的筆數，切換頁碼時 pagedEquipmentList 會自動更新 -->
    <el-pagination
      v-model:current-page="currentPage"
      :page-size="pageSize"
      :total="equipmentList.length"
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

.pagination {
  margin-top: 16px;
  justify-content: flex-end;
  flex-wrap: wrap;
}

/* 手機版（寬度 575px 以下）：篩選條件每個一行，下拉選單佔滿整行；分頁置中 */
@media (max-width: 575px) {
  .filter-form :deep(.el-form-item) {
    width: 100%;
    margin-right: 0;
  }

  .filter-select {
    width: 100%;
  }

  .pagination {
    justify-content: center;
  }
}
</style>
