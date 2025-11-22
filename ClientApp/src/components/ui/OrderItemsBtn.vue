<template>
  <div :class="['order-items-btn', className]" @click="goToCreateKey">
    <Key class="key-instance" />
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  import { useRouter } from 'vue-router'; // Import this
  import Key from '@/components/icons/Key.vue';
  export default defineComponent({
    name: 'OrderItemsBtn',
    components: { Key },
    props: {
      className: { type: String, default: '' },
    },
    setup() {
      const router = useRouter(); // Initialize router
      const goToCreateKey = () => {
        router.push('/seller/orderitems'); // Navigation method
      };
      return { goToCreateKey }; // Expose to template
    },
  });
</script>
<style scoped>
  .order-items-btn {
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

  .key-instance {
    height: 72px;
    width: 72px;
  }
</style>
