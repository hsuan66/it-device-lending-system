import type { Directive } from 'vue'

/**
 * 財產編號不允許的字元：英文大寫、數字、- 以外的都不行
 */
const disallowedPattern = /[^A-Z0-9-]/g

/**
 * 把文字整理成合法的財產編號：小寫轉大寫，再移除不允許的字元（空格、中文、符號）
 * @param value 輸入框目前的文字
 * @returns 整理後的文字，例如 "nb 2024-001" 會變成 "NB2024-001"
 */
function normalizeAssetCode(value: string): string {
  return value.toUpperCase().replace(disallowedPattern, '')
}

/**
 * 取得真正的 input 元素；掛在 el-input 上時拿到的是外層的 div，要往裡面找
 * @param el directive 掛上的元素
 * @returns input 元素；找不到時回傳 null
 */
function findInput(el: HTMLElement): HTMLInputElement | null {
  return el instanceof HTMLInputElement ? el : el.querySelector('input')
}

/**
 * 整理輸入框的內容，有變動才寫回，並通知 v-model 同步
 * @param input 要整理的輸入框
 * @returns 沒有回傳值
 */
function applyNormalize(input: HTMLInputElement) {
  const original = input.value
  const normalized = normalizeAssetCode(original)
  if (normalized === original) {
    return
  }

  // 游標位置：扣掉游標前面被移除的字元，避免每打一個字游標就跳到最後
  const caret = input.selectionStart ?? original.length
  const newCaret = normalizeAssetCode(original.slice(0, caret)).length

  input.value = normalized
  input.setSelectionRange(newCaret, newCaret)

  // 手動改 value 不會觸發 v-model，要再送一次 input 事件；這次值已經合法，不會無限循環
  input.dispatchEvent(new Event('input', { bubbles: true }))
}

/**
 * 輸入時整理內容；輸入法（例如注音）組字中先不處理，等組字結束再整理
 * @param event input 事件
 * @returns 沒有回傳值
 */
function handleInput(event: Event) {
  if ((event as InputEvent).isComposing) {
    return
  }
  applyNormalize(event.target as HTMLInputElement)
}

/**
 * 輸入法組字結束時整理內容
 * @param event compositionend 事件
 * @returns 沒有回傳值
 */
function handleCompositionEnd(event: Event) {
  applyNormalize(event.target as HTMLInputElement)
}

/**
 * 財產編號輸入限制：只能輸入英文大寫、數字和 -，小寫會自動轉成大寫
 * 用法：<el-input v-model="form.assetCode" v-asset-code />
 */
export const vAssetCode: Directive<HTMLElement> = {
  mounted(el) {
    const input = findInput(el)
    input?.addEventListener('input', handleInput)
    input?.addEventListener('compositionend', handleCompositionEnd)
  },
  unmounted(el) {
    const input = findInput(el)
    input?.removeEventListener('input', handleInput)
    input?.removeEventListener('compositionend', handleCompositionEnd)
  },
}
