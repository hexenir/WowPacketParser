<script lang="ts" setup>
import type {Packet} from "../types";
import {computed} from "vue";

const props = defineProps<{
    packet: Packet
}>()

const clsMap = [
    [(match: any) => match.startsWith('"') && match.endsWith(':'), "red"],
    [(match: any) => match.startsWith('"'), "green"],
    [(match: any) => match === "true" || match === "false", "blue"],
    [(match: any) => match === "null", "magenta"],
    [() => true, "darkorange"],
];

const packetString = computed(() => JSON.stringify(props.packet, null, 4)
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/("(\\u[a-zA-Z0-9]{4}|\\[^u]|[^\\"])*"(\s*:)?|\b(true|false|null)\b|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?)/g, match => `<span style="color:${clsMap.find(([fn]) => (fn as any)(match))?.[1]}">${match}</span>`)
);
</script>
<template>
    <div class="columns">
        <div class="column">
            <div class="card">
                <header class="card-header">
                    <p class="card-header-title">Details</p>
                </header>
                <div
                    class="content"
                    style="white-space: break-spaces"
                    v-html="packetString"
                />
            </div>
        </div>
        <div class="column">
            <div class="card">
                <header class="card-header">
                    <p class="card-header-title">Message</p>
                </header>
                <div
                    class="content"
                    style="white-space: pre-line"
                >
                    {{ props.packet.message }}
                </div>
            </div>
        </div>
    </div>
</template>