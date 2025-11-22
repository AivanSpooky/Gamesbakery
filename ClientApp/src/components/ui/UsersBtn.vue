<template>
  <div :class="['users-btn', className]" @click="navigateToUsers">
    <Group class="group-instance" />
  </div>
</template>

<script setup lang="ts">
  import { defineProps } from 'vue'; // If not already imported
  import { useRouter } from 'vue-router';
  import Group from '@/components/icons/Group.vue';

  const props = defineProps({
    className: { type: String, default: '' },
  });

  const router = useRouter();

  const navigateToUsers = () => {
    router.push('/admin/users'); 
  };
</script>

<style scoped>
  .users-btn {
    background-color: var(--color-background-accent);
    border: var(--border-interactive) solid var(--color-border-1);
    border-radius: var(--radius-main);
    display: flex;
    align-items: center;
    justify-content: center;
    height: 120px;
    width: 122px;
    overflow: hidden;
    cursor: pointer;
  }

  .group-instance {
    height: 72px;
    width: 72px;
  }
</style>
