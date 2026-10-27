<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Delete, Edit, MoreFilled } from '@element-plus/icons-vue'
import BorrowDialog from '@/components/BorrowDialog.vue'
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
import { hasFilterValue } from '@/utils/filter'

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
 * 「借用」按鈕停用時 tooltip 顯示的原因；可借用時 tooltip 不顯示，所以留空
 */
const borrowDisabledReasons: Record<EquipmentStatus, string> = {
  1: '',
  2: '設備借用中，無法借出',
  3: '設備逾期未還，無法借出',
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
 * 最近一次載入設備清單是否失敗；失敗時表格顯示「資料載入失敗」與重新整理按鈕
 */
const loadFailed = ref(false)

/**
 * 最近一次查詢是否有帶篩選條件；用來區分「完全沒有設備」和「篩選後沒有符合的設備」
 */
const filtered = ref(false)

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
 * 借用設備的對話框是否顯示
 */
const borrowDialogVisible = ref(false)

/**
 * 要借用的設備
 */
const borrowingEquipment = ref<Equipment | null>(null)

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
  // 記下這次查詢有沒有篩選條件
  filtered.value = [query.typeId, query.status].some(hasFilterValue)
  try {
    equipmentList.value = await getEquipmentList(query)
    loadFailed.value = false

    if (keepPage) {
      // 刪除後總頁數可能變少，頁碼不能超過最後一頁
      const lastPage = Math.max(1, Math.ceil(equipmentList.value.length / pageSize))
      currentPage.value = Math.min(currentPage.value, lastPage)
    } else {
      // 篩選條件可能變了，回到第 1 頁
      currentPage.value = 1
    }
  } catch (error) {
    // 清空舊資料，避免畫面上留著可能已經過時的清單
    equipmentList.value = []
    loadFailed.value = true
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
 * 打開借用設備的對話框
 * @param equipment 要借用的設備
 * @returns 沒有回傳值
 */
function openBorrowDialog(equipment: Equipment) {
  borrowingEquipment.value = equipment
  borrowDialogVisible.value = true
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
 * 處理「⋯」選單的項目：修改或刪除
 * @param command 被點選的選單項目，edit 或 delete
 * @param equipment 這一列的設備
 * @returns 沒有回傳值
 */
function handleRowCommand(command: string | number | object, equipment: Equipment) {
  if (command === 'edit') {
    openEditDialog(equipment)
  } else if (command === 'delete') {
    handleDelete(equipment)
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
    >
      <!-- 沒有資料時，依情況顯示不同的說明；載入中不顯示，避免文字在轉圈圈底下閃一下 -->
      <template #empty>
        <template v-if="!loading">
          <el-empty v-if="loadFailed" :image-size="80" description="資料載入失敗">
            <el-button type="primary" @click="loadEquipmentList()">重新整理</el-button>
          </el-empty>
          <el-empty v-else-if="filtered" :image-size="80" description="沒有符合條件的設備" />
          <el-empty v-else :image-size="80" description="目前沒有設備，請點右上角「新增設備」建立" />
        </template>
      </template>

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
      <el-table-column label="操作" width="140" align="center" fixed="right">
        <template #default="{ row }">
          <!--
            主要動作「借用」單獨顯示；次要動作「修改」「刪除」收進「⋯」選單
            兩顆都用預設按鈕（淺灰框、白底），避免跟狀態欄的彩色標籤混淆
          -->
          <div class="row-actions">
            <!--
              只有可借用的設備能借出；後端也會擋，這裡先停用按鈕並用 tooltip 說明原因
              按鈕外面包一層 span：停用的按鈕不會觸發滑鼠事件，tooltip 要掛在 span 上才顯示得出來
            -->
            <el-tooltip
              :content="borrowDisabledReasons[(row as Equipment).status]"
              placement="top"
              :disabled="(row as Equipment).status === EquipmentStatus.Available"
            >
              <span>
                <el-button
                  size="small"
                  :disabled="(row as Equipment).status !== EquipmentStatus.Available"
                  @click="openBorrowDialog(row as Equipment)"
                >
                  借用
                </el-button>
              </span>
            </el-tooltip>

            <!-- trigger="click"：點擊展開，手機也能操作 -->
            <el-dropdown trigger="click" @command="(command) => handleRowCommand(command, row as Equipment)">
              <el-button size="small" :icon="MoreFilled" aria-label="更多操作" />
              <template #dropdown>
                <el-dropdown-menu>
                  <el-dropdown-item :icon="Edit" command="edit">修改</el-dropdown-item>
                  <!--
                    借用中、逾期的設備不能刪除；跟借用按鈕一樣停用並用 tooltip 說明原因
                    停用的選單項目仍會觸發滑鼠事件，tooltip 可以直接掛在裡面的 span 上
                  -->
                  <el-dropdown-item
                    command="delete"
                    divided
                    :disabled="(row as Equipment).status !== EquipmentStatus.Available"
                    class="danger-item"
                  >
                    <!--
                      placement="left"：選單在表格最右邊，往左顯示才不會被畫面切掉
                      icon 不用 :icon prop，改放在 span 裡，tooltip 才會對齊整個項目的左邊、不蓋住 icon
                    -->
                    <el-tooltip
                      content="設備尚未歸還，無法刪除"
                      placement="left"
                      :disabled="(row as Equipment).status === EquipmentStatus.Available"
                    >
                      <span class="menu-item-label">
                        <el-icon><Delete /></el-icon>
                        刪除
                      </span>
                    </el-tooltip>
                  </el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>
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

    <BorrowDialog
      v-model="borrowDialogVisible"
      :equipment="borrowingEquipment"
      @borrowed="loadEquipmentList(true)"
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

/* 操作欄：「借用」和「⋯」兩顆按鈕並排置中；借用按鈕外面包了 span，Element Plus 預設的按鈕間距會失效，改用 gap */
.row-actions {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 8px;
}

/* 選單裡的「刪除」用紅字表示有破壞性；停用時維持 Element Plus 的灰色 */
.danger-item:not(.is-disabled) {
  color: var(--el-color-danger);
}

/*
 * 選單項目的內容（icon＋文字）撐滿整個項目：用負 margin 抵銷項目 5px 16px 的 padding，再用 padding 補回
 * 滑鼠移到項目任何位置都會出現 tooltip，tooltip 也會出現在選單外面、不蓋住 icon
 */
.menu-item-label {
  display: flex;
  align-items: center;
  flex: 1;
  margin: -5px -16px;
  padding: 5px 16px;
}

/* 滑鼠移上去時 Element Plus 預設會變藍色，刪除改成維持紅字、淡紅色背景 */
.danger-item:not(.is-disabled):hover,
.danger-item:not(.is-disabled):focus {
  color: var(--el-color-danger);
  background-color: var(--el-color-danger-light-9);
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
