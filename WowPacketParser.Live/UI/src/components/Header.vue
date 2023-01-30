<script lang="ts" setup>
import {chain, isEmpty} from "lodash";
import type {Packet} from "../types";
import {computed, Ref, ref} from "vue";

enum ConnectionStatus {
    Open,
    Closed,
    Connecting
}

const emit = defineEmits<{
    (e: "packet", v: Packet): void
    (e: "clear"): void
}>()

const status: Ref<ConnectionStatus> = ref(ConnectionStatus.Closed);
const host = ref("127.0.0.1")
const port = ref(8888);
const blacklist = ref('')
const whitelist = ref('')

const url = computed(() => `ws://${host.value}:${port.value}`)

const rejected = computed(() => chain(blacklist.value.split(',')).reject(isEmpty).map(parseInt).value())
const accepted = computed(() => chain(whitelist.value.split(',')).reject(isEmpty).map(parseInt).value())

let socket: WebSocket;

function connect() {
    status.value = ConnectionStatus.Connecting
    socket = new WebSocket(url.value);
    socket.addEventListener('open', e => {
        status.value = ConnectionStatus.Open
    })
    socket.addEventListener('error', e => {
        console.error(e)
        status.value = ConnectionStatus.Closed
    })
    socket.addEventListener('close', e => {
        status.value = ConnectionStatus.Closed
    })
    socket.addEventListener('message', e => {
        const packet: Packet = JSON.parse(e.data)
        if (accepted.value.length > 0 && !accepted.value.includes(packet.opCode) ||
            rejected.value.includes(packet.opCode)
        ) {
            return;
        }
        emit('packet', packet)
    })
}

function disconnect() {
    socket.close();
    status.value = ConnectionStatus.Closed
}


</script>
<template>
    <div class="has-background-grey-lighter pb-3">
        <div class="columns is-vcentered">
            <div
                v-if="status === ConnectionStatus.Closed"
                class="column is-flex is-one-third"
            >
                <input
                    v-model="host"
                    class="input"
                    placeholder="Host"
                    type="text"
                >
                <input
                    v-model="port"
                    class="input mr-3"
                    placeholder="Port"
                    type="number"
                >
                <button
                    class="button is-primary"
                    @click="connect">Connect
                </button>
            </div>
            <div
                v-else-if="status === ConnectionStatus.Open"
                class="column is-flex"
            >
                <button
                    class="button is-link pr-3"
                    @click="disconnect"
                >
                    Disconnect
                </button>
                <p> Connected to {{ url }}</p>
            </div>
            <p v-else class="is-warning">Connecting...</p>
            <div class="column is-one-quarter">
                <label for="blacklist">Blacklist</label>
                <input
                    id="blacklist"
                    v-model="blacklist"
                    class="input"
                    type="text"
                >
            </div>
            <div class="column is-one-quarter">
                <label for="whitelist">Whitelist</label>
                <input
                    id="whitelist"
                    v-model="whitelist"
                    class="input"
                    type="text"
                >
            </div>
            <div class="column">
                <button class="button is-danger p-3"
                        @click="emit('clear')"
                >
                    Clear
                </button>
            </div>
        </div>
    </div>
</template>
