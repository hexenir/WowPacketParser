<script lang="ts" setup>
import type {Packet} from "../types";
import PacketSummary from "./PacketSummary.vue";
import {computed} from "vue";

const props = defineProps<{
    packets: Packet[],
    selected?: Packet
}>()

const emit = defineEmits<{
    (e: "update:selected", v: Packet | undefined): void
}>()

const selected = computed({
    get: () => props.selected,
    set: v => emit('update:selected', v)
})

function handleClick(packet: Packet) {
    selected.value = packet
}
</script>
<template>
    <div class="box" style="max-height: 500px; overflow: auto">
        <PacketSummary
            v-for="i in props.packets.length"
            :key="i"
            :active="props.packets[packets.length - i].number === selected?.number"
            :packet="props.packets[packets.length - i]"
            @click="handleClick(props.packets[packets.length - i])"
        />
    </div>
</template>