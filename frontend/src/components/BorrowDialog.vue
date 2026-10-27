<script setup lang="ts">
import { reactive, ref } from 'vue'
import { ElMessage, type FormInstance, type FormRules } from 'element-plus'
import { createBorrowRecord } from '@/api/borrowRecord'
import type { Equipment } from '@/types/equipment'
import { toErrorMessage } from '@/utils/error'

/**
 * 元件的參數
 */
const props = defineProps<{
  /** 要借用的設備；對話框關閉時可以是 null */
  equipment: Equipment | null
}>()

/**
 * 元件發出的事件
 */
const emit = defineEmits<{
  /** 借用成功，父元件收到後重新載入清單 */
  borrowed: []
}>()

/**
 * 對話框是否顯示，由父元件用 v-model 控制
 */
const visible = defineModel<boolean>({ required: true })

/**
 * 借用表單的資料；預計歸還時間還沒選時是 undefined
 */
interface BorrowFormModel {
  borrowerName: string
  expectReturnTime: Date | undefined
}

/**
 * 表單目前填寫的內容
 */
const form = reactive<BorrowFormModel>({
  borrowerName: '',
  expectReturnTime: undefined,
})

/**
 * 表單驗證規則；借用人長度對齊資料表欄位，預計歸還時間必須晚於現在
 */
const rules: FormRules<BorrowFormModel> = {
  borrowerName: [
    { required: true, whitespace: true, message: '請輸入借用人姓名', trigger: 'blur' },
    { max: 50, message: '借用人姓名最多 50 個字', trigger: 'blur' },
  ],
  expectReturnTime: [
    { required: true, message: '請選擇預計歸還時間', trigger: 'change' },
    {
      validator: (_rule, value: Date | undefined, callback) => {
        if (value && value.getTime() <= Date.now()) {
          callback(new Error('預計歸還時間必須晚於現在'))
        } else {
          callback()
        }
      },
      trigger: 'change',
    },
  ],
}

/**
 * 日期選擇器選完日期時預設帶入的時間：18:00
 */
const defaultTime = new Date(2000, 0, 1, 18, 0, 0)

/**
 * el-form 元件，用來執行驗證、清除驗證訊息
 */
const formRef = ref<FormInstance>()

/**
 * 是否正在送出，避免重複送出
 */
const submitting = ref(false)

/**
 * 日期選擇器中，今天以前的日期不能選
 * @param date 日曆上的某一天
 * @returns true 代表這一天不能選
 */
function isBeforeToday(date: Date) {
  const today = new Date()
  today.setHours(0, 0, 0, 0)
  return date.getTime() < today.getTime()
}

/**
 * 對話框打開時清空表單
 * @returns 沒有回傳值
 */
function handleOpen() {
  form.borrowerName = ''
  form.expectReturnTime = undefined
  // 清掉上一次留下的驗證錯誤訊息
  formRef.value?.clearValidate()
}

/**
 * 驗證表單後送出借用
 * @returns 沒有回傳值；成功時關閉對話框並通知父元件，失敗時顯示錯誤訊息
 */
async function handleSubmit() {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid || !props.equipment || !form.expectReturnTime) {
    return
  }

  submitting.value = true
  try {
    await createBorrowRecord({
      equipmentId: props.equipment.id,
      borrowerName: form.borrowerName.trim(),
      // toISOString() 會轉成 UTC，例如 2026-11-02T10:00:00.000Z；借出時間由後端以目前時間寫入
      expectReturnTime: form.expectReturnTime.toISOString(),
    })
    ElMessage.success('借用成功')
    visible.value = false
    emit('borrowed')
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
    title="借用設備"
    width="min(480px, 92vw)"
    :close-on-click-modal="false"
    @open="handleOpen"
  >
    <el-form ref="formRef" :model="form" :rules="rules" label-width="100px" @submit.prevent="handleSubmit">
      <el-form-item label="設備">
        <span>{{ equipment?.assetCode }} {{ equipment?.name }}</span>
      </el-form-item>

      <el-form-item label="借用人" prop="borrowerName">
        <el-input v-model="form.borrowerName" maxlength="50" placeholder="請輸入借用人姓名" />
      </el-form-item>

      <el-form-item label="預計歸還" prop="expectReturnTime">
        <el-date-picker
          v-model="form.expectReturnTime"
          type="datetime"
          format="YYYY/MM/DD HH:mm"
          placeholder="選擇日期與時間"
          :default-time="defaultTime"
          :disabled-date="isBeforeToday"
          class="date-picker"
        />
      </el-form-item>
    </el-form>

    <template #footer>
      <el-button @click="visible = false">取消</el-button>
      <el-button type="primary" :loading="submitting" @click="handleSubmit">借用</el-button>
    </template>
  </el-dialog>
</template>

<style scoped>
.date-picker {
  width: 100%;
}
</style>
