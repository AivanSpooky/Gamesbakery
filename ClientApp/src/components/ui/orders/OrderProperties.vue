<template>
  <div :class="['order-properties', className]">
    <OrderTitle :text="title" />
    <div class="filters flex items-center gap-4">
      <StatusLabel :text="statusLblText" />
      <StatusValue :text="statusText" />
      <DateLabel :text="dateLblText" />
      <DateValue :text="dateText" />
      <PriceValue :text="priceText" />
    </div>
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  import OrderTitle from '@/components/ui/orders/OrderTitle.vue';
  import StatusLabel from '@/components/ui/orders/StatusLabel.vue';
  import StatusValue from '@/components/ui/orders/StatusValue.vue';
  import DateLabel from '@/components/ui/orders/DateLabel.vue';
  import DateValue from '@/components/ui/orders/DateValue.vue';
  import PriceValue from '@/components/ui/orders/PriceValue.vue';
  export default defineComponent({
    name: 'OrderProperties',
    components: { OrderTitle, StatusLabel, StatusValue, DateLabel, DateValue, PriceValue },
    props: {
      className: { type: String, default: '' },
      title: { type: String, default: 'Детали заказа' },
      statusLblText: { type: String, default: 'Статус:' },
      statusText: { type: String, default: 'оплачен' },
      dateLblText: { type: String, default: 'Дата выхода:' },
      dateText: { type: String, default: '11/12/2024' },
      priceText: { type: String, default: '498' },
    },
  });
</script>
<style scoped>
  .order-properties {
    background-color: var(--color-background);
    border: var(--border-interactive) solid var(--color-border-1);
    border-radius: var(--radius-main);
    display: flex;
    flex-direction: column;
    width: 100%;
  }

  .filters {
    display: flex;
    align-items: center;
    gap: 20px;
    padding: 10px 20px;
  }
</style>
