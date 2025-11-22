<template>
  <div :class="['user-item', className, 'flex', 'items-center', 'gap-4']">
    <div class="table-seller-lbl">{{ user.username }}</div>
    <div class="table-price-lbl">{{ user.email }}</div>
    <div class="table-delete-btn unban" @click="$emit('unban')">Разбанить</div>
    <div class="table-delete-btn ban" @click="$emit('ban')">Забанить</div>
  </div>
</template>
<script setup lang="ts">
  defineProps<{
    user: { id: string; username: string; email: string; role: string };
    className?: string;
  }>();
  defineEmits(['ban', 'unban']);
</script>
<style scoped>
  .user-item {
    display: flex; /* Ensures flex container behavior */
    flex-direction: row; /* Explicitly sets horizontal alignment (default, but good to specify) */
    align-items: center; /* Vertically centers items in the row */
    gap: 16px; /* Spacing between items; adjust as needed (equivalent to gap-4 in Tailwind) */
    flex-wrap: nowrap; /* Prevents wrapping to new lines on smaller screens */
    background-color: var(--color-background);
    border: 12px solid var(--color-background-accent);
    border-radius: var(--radius-main);
    padding: 18px 30px;
  }

  .table-seller-lbl {
    background-color: var(--color-gift-accent); /* Purple from snippet */
    border: 6px solid var(--color-pink-border);
    border-radius: var(--radius-main);
    padding: 10px 20px;
    color: var(--color-text-primary);
    width: 100%;
  }

  .table-price-lbl {
    background-color: var(--color-teal-accent); /* Teal */
    border: 6px solid var(--color-teal-border);
    border-radius: var(--radius-main);
    padding: 10px 20px;
    color: var(--color-text-primary);
    width: 100%;
  }

  .table-delete-btn {
    border-radius: var(--radius-main);
    padding: 10px 20px;
    color: var(--color-text-primary);
    cursor: pointer;
    width: 100%;
  }

  .ban {
    background-color: var(--color-ban-color);
    border: 12px solid var(--color-ban-border);
  }

  .unban {
    background-color: var(--color-unban-color);
    border: 12px solid var(--color-unban-border);
  }
</style>
