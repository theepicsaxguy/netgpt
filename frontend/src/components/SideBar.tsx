import React from 'react';
import { useGetApiV1Conversations, useDeleteApiV1ConversationsId, useGetApiHealth } from '../api/v1/generated/api';
import { ConversationDto, PagedResult, castTo } from '../types';
import { Plus, MessageSquare, Trash2, Loader2, RefreshCw, Terminal, Activity, Search } from 'lucide-react';
import { useQueryClient } from '@tanstack/react-query';

interface SidebarProps {
  selectedId: string | null;
  onSelect: (id: string) => void;
  onNewChat: () => void;
}

const Sidebar: React.FC<SidebarProps> = ({ selectedId, onSelect, onNewChat }) => {
  const queryClient = useQueryClient();
  const { data: rawData, isLoading, isError, refetch } = useGetApiV1Conversations({
    page: 1,
    pageSize: 50
  });

  const { isSuccess: isHealthSuccess } = useGetApiHealth({
    query: { refetchInterval: 30000 }
  });

  const { mutate: deleteConversation } = useDeleteApiV1ConversationsId({
    mutation: {
      onSuccess: (_: unknown, variables: { id: string }) => {
        queryClient.invalidateQueries({ queryKey: ['/api/v1/v1/Conversations'] });
        if (selectedId === variables.id) {
          onSelect('');
        }
      }
    }
  });

  const data = castTo<PagedResult<ConversationDto> | ConversationDto[]>(rawData);
  const items = Array.isArray(data) ? data : data?.items || [];

  return (
    <aside className="w-[300px] bg-black border-r border-zinc-800 flex flex-col h-full shrink-0 relative z-20">
      <div className="p-4 flex flex-col gap-4">
        <div className="flex items-center justify-between px-2 pt-2">
          <div className="flex items-center gap-3">
            <div className="w-8 h-8 bg-zinc-100 rounded-md flex items-center justify-center shadow-lg shadow-white/5">
               <Terminal size={16} className="text-black" />
            </div>
            <h2 className="text-lg font-bold text-zinc-100 tracking-tight">
              NetGPT
            </h2>
          </div>
          <div className="flex items-center gap-1.5 px-2 py-1 rounded-full bg-zinc-900 border border-zinc-800">
            <div className={`w-1.5 h-1.5 rounded-full ${isHealthSuccess ? 'bg-emerald-500 shadow-[0_0_8px_rgba(16,185,129,0.5)]' : 'bg-red-500'}`}></div>
            <span className="text-[10px] text-zinc-500 font-medium uppercase tracking-wider">
              {isHealthSuccess ? 'Online' : 'Offline'}
            </span>
          </div>
        </div>

        <button 
          onClick={onNewChat}
          className="w-full py-2.5 px-4 bg-zinc-900 hover:bg-zinc-800 border border-zinc-800 hover:border-zinc-700 rounded-lg text-zinc-300 hover:text-white transition-all duration-200 flex items-center justify-start gap-3 group"
        >
          <Plus size={16} className="text-zinc-500 group-hover:text-zinc-100 transition-colors" />
          <span className="font-medium text-sm">New Session</span>
        </button>
      </div>

      <div className="flex-1 overflow-y-auto px-3 pb-3 space-y-1 custom-scrollbar">
        {isLoading ? (
          <div className="flex flex-col items-center justify-center h-32 gap-3 text-zinc-600">
            <Loader2 className="animate-spin" size={16} />
            <span className="text-xs font-mono">LOADING_DATA...</span>
          </div>
        ) : isError ? (
          <div className="flex flex-col items-center justify-center h-32 gap-3 text-zinc-500 p-4 text-center">
            <p className="text-xs">Connection failed</p>
            <button 
              onClick={() => refetch()}
              className="flex items-center gap-2 px-3 py-1.5 bg-zinc-900 rounded-md text-xs hover:bg-zinc-800 transition-colors border border-zinc-800"
            >
              <RefreshCw size={12} /> Retry
            </button>
          </div>
        ) : items.length === 0 ? (
          <div className="flex flex-col items-center justify-center h-48 text-zinc-700 gap-2">
            <Search size={24} className="opacity-20" />
            <span className="text-xs">No conversations found</span>
          </div>
        ) : (
          <>
            <div className="px-3 py-3 text-[10px] font-bold text-zinc-600 uppercase tracking-widest font-mono">History</div>
            {items.map((conv) => (
              <div 
                key={conv.id}
                className={`group relative flex items-center justify-between p-2.5 rounded-lg cursor-pointer transition-all duration-200 border ${
                  selectedId === conv.id 
                    ? 'bg-zinc-900 border-zinc-700 text-zinc-100' 
                    : 'bg-transparent border-transparent text-zinc-400 hover:bg-zinc-900/50 hover:text-zinc-300'
                }`}
                onClick={() => onSelect(conv.id)}
              >
                <div className="flex items-center gap-3 overflow-hidden">
                  <MessageSquare size={14} className={`shrink-0 transition-colors ${selectedId === conv.id ? 'text-zinc-100' : 'text-zinc-600 group-hover:text-zinc-500'}`} />
                  <span className="truncate text-sm font-medium pr-6">
                    {conv.title || 'Untitled Session'}
                  </span>
                </div>
                
                <button
                  onClick={(e) => {
                    e.stopPropagation();
                    if(confirm('Terminate this session?')) {
                      deleteConversation({ id: conv.id });
                    }
                  }}
                  className={`absolute right-2 p-1.5 rounded opacity-0 group-hover:opacity-100 transition-all hover:bg-red-500/10 hover:text-red-400 ${
                    selectedId === conv.id ? 'text-zinc-500' : 'text-zinc-700'
                  }`}
                  title="Delete session"
                >
                  <Trash2 size={12} />
                </button>
              </div>
            ))}
          </>
        )}
      </div>

      <div className="p-4 border-t border-zinc-800 bg-black text-xs text-zinc-600 flex justify-between items-center">
        <span className="font-mono text-[10px]">NETGPT v1.0</span>
        <Activity size={12} className={isHealthSuccess ? "text-emerald-500/50" : "text-zinc-800"} />
      </div>
    </aside>
  );
};

export default Sidebar;