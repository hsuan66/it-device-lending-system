<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import { ElMessage, type FormInstance, type FormRules } from 'element-plus'
import { createEquipment, updateEquipment } from '@/api/equipment'
import type { Equipment } from '@/types/equipment'
import type { EquipmentType } from '@/types/equipmentType'
import { toErrorMessage } from '@/utils/error'

/**
 * 元件的參數
 */
const props = defineProps<{
  /** 要修改的設備；傳 null 代表新增 */
  equipment: Equipment | null
  /** 設備類型下拉選單的選項 */
  equipmentTypes: EquipmentType[]
}>()

/**
 * 元件發出的事件
 */
const emit = defineEmits<{
  /** 新增或修改成功，父元件收到後重新載入清單 */
  saved: []
}>()

/**
 * 對話框是否顯示，由父元件用 v-model 控制
 */
const visible = defineModel<boolean>({ required: true })

/**
 * 表單資料；類型還沒選時是 undefined
 */
interface EquipmentFormModel {
  assetCode: string
  name: string
  equipmentTypeId: number | undefined
}

/**
 * 表單目前填寫的內容
 */
const form = reactive<EquipmentFormModel>({
  assetCode: '',
  name: '',
  equipmentTypeId: undefined,
})

/**
 * 表單驗證規則，長度限制對齊資料表欄位
 */
const rules: FormRules<EquipmentFormModel> = {
  assetCode: [
    { required: true, whitespace: true, message: '請輸入財產編號', trigger: 'blur' },
    { max: 30, message: '財產編號最多 30 個字', trigger: 'blur' },
  ],
  name: [
    { required: true, whitespace: true, message: '請輸入設備名稱', trigger: 'blur' },
    { max: 100, message: '設備名稱最多 100 個字', trigger: 'blur' },
  ],
  equipmentTypeId: [{ required: true, message: '請選擇設備類型', trigger: 'change' }],
}

/**
 * el-form 元件，用來執行驗證、清除驗證訊息
 */
const formRef = ref<FormInstance>()

/**
 * 是否正在送出，避免重複送出
 */
const submitting = ref(false)

/**
 * 是否為修改模式
 */
const isEdit = computed(() => props.equipment !== null)

/**
 * 對話框標題
 */
const title = computed(() => (isEdit.value ? '修改設備' : '新增設備'))

/**
 * 對話框打開時，依新增或修改填入表單初始值
 * @returns 沒有回傳值
 */
function handleOpen() {
  form.assetCode = props.equipment?.assetCode ?? ''
  form.name = props.equipment?.name ?? ''
  form.equipmentTypeId = props.equipment?.equipmentTypeId
  // 清掉上一次留下的驗證錯誤訊息
  formRef.value?.clearValidate()
}

/**
 * 驗證表單後送出新增或修改
 * @returns 沒有回傳值；成功時關閉對話框並通知父元件，失敗時顯示錯誤訊息
 */
async function handleSubmit() {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid || form.equipmentTypeId === undefined) {
    return
  }

  const data = {
    assetCode: form.assetCode.trim(),
    name: form.name.trim(),
    equipmentTypeId: form.equipmentTypeId,
  }

  submitting.value = true
  try {
    if (props.equipment) {
      await updateEquipment(props.equipment.id, data)
      ElMessage.success('修改成功')
    } else {
      await createEquipment(data)
      ElMessage.success('新增成功')
    }
    visible.value = false
    emit('saved')
  } catch (error) {
    ElMessage.error(toErrorMessage(error))
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <el-dialog
    v-model="visible"
    :title="title"
    width="min(480px, 92vw)"
    :close-on-click-modal="false"
    @open="handleOpen"
  >
    <el-form ref="formRef" :model="form" :rules="rules" label-width="80px" @submit.prevent="handleSubmit">
      <el-form-item label="財產編號" prop="assetCode">
        <el-input v-model="form.assetCode" maxlength="30" placeholder="例如 NB-2025-001" />
      </el-form-item>

      <el-form-item label="設備名稱" prop="name">
        <el-input v-model="form.name" maxlength="100" placeholder="例如 Lenovo ThinkPad T14" />
      </el-form-item>

      <el-form-item label="設備類型" prop="equipmentTypeId">
        <el-select v-model="form.equipmentTypeId" placeholder="請選擇">
          <el-option
            v-for="equipmentType in equipmentTypes"
            :key="equipmentType.id"
            :label="equipmentType.name"
            :value="equipmentType.id"
          />
        </el-select>
      </el-form-item>
    </el-form>

    <template #footer>
      <el-button @click="visible = false">取消</el-button>
      <el-button type="primary" :loading="submitting" @click="handleSubmit">儲存</el-button>
    </template>
  </el-dialog>
</template>
