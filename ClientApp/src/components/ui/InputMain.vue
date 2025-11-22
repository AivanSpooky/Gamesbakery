<template>
  <div class="input-main">
    <InputLbl :text="label" />
    <input v-model="model" :type="type" :placeholder="placeholder" class="neon-input w-full" />
  </div>
</template>

<script setup lang="ts">
  import { computed } from 'vue';
  import InputLbl from './InputLbl.vue'; // Assume from your code

  const props = defineProps<{
    label: string;
    placeholder: string;
    type?: string;
  }>();

  const model = defineModel<string>(); // For v-model support
</script>

<style scoped>
  .input-main {
    display: flex;
    flex-direction: column;
    gap: 8px;
  }
</style>
