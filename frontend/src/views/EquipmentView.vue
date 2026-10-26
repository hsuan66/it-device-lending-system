<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Delete, Edit } from '@element-plus/icons-vue'
import EquipmentFormDialog from '@/components/EquipmentFormDialog.vue'
import { deleteEquipment, getEquipmentList } from '@/api/equipment'
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
 * 新增／修改設備的對話框是否顯示
 */
const formDialogVisible = ref(false)

/**
 * 對話框要修改的設備；null 代表新增
 */
const editingEquipment = ref<Equipment | null>(null)

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
 * @param keepPage 是否停留在目前的頁碼；新增、修改、刪除後重新載入時傳 true
 * @returns 沒有回傳值；失敗時顯示錯誤訊息
 */
async function loadEquipmentList(keepPage = false) {
  loading.value = true
  try {
    equipmentList.value = await getEquipmentList(query)

    if (keepPage) {
      // 刪除後總頁數可能變少，頁碼不能超過最後一頁
      const lastPage = Math.max(1, Math.ceil(equipmentList.value.length / pageSize))
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
 * 打開新增設備的對話框
 * @returns 沒有回傳值
 */
function openCreateDialog() {
  editingEquipment.value = null
  formDialogVisible.value = true
}

/**
 * 打開修改設備的對話框
 * @param equipment 要修改的設備
 * @returns 沒有回傳值
 */
function openEditDialog(equipment: Equipment) {
  editingEquipment.value = equipment
  formDialogVisible.value = true
}

/**
 * 確認後刪除設備（軟刪除）
 * @param equipment 要刪除的設備
 * @returns 沒有回傳值；按取消時不做事，失敗時顯示錯誤訊息
 */
async function handleDelete(equipment: Equipment) {
  try {
    await ElMessageBox.confirm(
      `確定要刪除「${equipment.assetCode} ${equipment.name}」嗎？`,
      '刪除設備',
      { type: 'warning', confirmButtonText: '刪除', cancelButtonText: '取消' },
    )
  } catch {
    // 按取消或關閉確認框
    return
  }

  try {
    await deleteEquipment(equipment.id)
    ElMessage.success('刪除成功')
    await loadEquipmentList(true)
  } catch (error) {
    ElMessage.error(toErrorMessage(error))
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
    <template #header>
      <div class="card-header">
        <span>設備管理</span>
        <el-button type="primary" @click="openCreateDialog">新增設備</el-button>
      </div>
    </template>

    <!-- 篩選條件：按 Enter 或「查詢」都會重新載入 -->
    <el-form :model="query" inline class="filter-form" @submit.prevent="loadEquipmentList()">
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
      <el-table-column label="操作" width="100" align="center" fixed="right">
        <template #default="{ row }">
          <!--
            el-tooltip 在滑鼠移上去時顯示文字說明，aria-label 給螢幕閱讀器使用
            按鈕外面包一層 span：停用的按鈕不會觸發滑鼠事件，tooltip 要掛在 span 上才顯示得出來
          -->
          <div class="row-actions">
            <el-tooltip content="修改" placement="top">
              <span>
                <el-button
                  link
                  type="primary"
                  :icon="Edit"
                  aria-label="修改"
                  @click="openEditDialog(row as Equipment)"
                />
              </span>
            </el-tooltip>

            <!-- 借用中、逾期的設備不能刪除；後端也會擋，這裡先停用按鈕並說明原因 -->
            <el-tooltip
              :content="(row as Equipment).status === EquipmentStatus.Available ? '刪除' : '借用中的設備不能刪除'"
              placement="top"
            >
              <span>
                <el-button
                  link
                  type="danger"
                  :icon="Delete"
                  aria-label="刪除"
                  :disabled="(row as Equipment).status !== EquipmentStatus.Available"
                  @click="handleDelete(row as Equipment)"
                />
              </span>
            </el-tooltip>
          </div>
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

    <EquipmentFormDialog
      v-model="formDialogVisible"
      :equipment="editingEquipment"
      :equipment-types="equipmentTypes"
      @saved="loadEquipmentList(true)"
    />
  </el-card>
</template>

<style scoped>
.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.filter-select {
  width: 160px;
}

/* 操作欄的兩個 icon 按鈕：外面包了 span，Element Plus 預設的按鈕間距會失效，改用 gap */
.row-actions {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 12px;
}

/* icon 從預設的 14px（跟著按鈕字體）放大到 18px */
.row-actions :deep(.el-icon) {
  font-size: 18px;
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
