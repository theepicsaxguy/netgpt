import React, { useEffect, useRef } from 'react';
import { useGetApiV1ConversationsConversationIdMessages, useGetApiV1ConversationsId } from '../api/v1/generated/api';
import { MessageDto, ConversationDto, castTo } from '../types';
import InputArea from './InputArea';
import MessageBubble from './MessageBubble';
import { Loader2, Bot, Command, Hash, Info } from 'lucide-react';

interface ChatAreaProps {
  conversationId: string;
}

const ChatArea: React.FC<ChatAreaProps> = ({ conversationId }) => {
  const scrollRef = useRef<HTMLDivElement>(null);
  const bottomRef = useRef<HTMLDivElement>(null);
  
  // Fetch messages
  const { data: rawMessagesData, isLoading: isMessagesLoading, isError: isMessagesError, refetch: refetchMessages } = useGetApiV1ConversationsConversationIdMessages(conversationId, {
    query: {
        refetchInterval: 3000, 
    }
  });

  // Fetch conversation details for header info
  const { data: rawConversationData } = useGetApiV1ConversationsId(conversationId);

  const messages = castTo<MessageDto[]>(rawMessagesData) || [];
  const conversation = castTo<ConversationDto>(rawConversationData);
  
  // Smart auto-scroll
  useEffect(() => {
    if (bottomRef.current) {
        bottomRef.current.scrollIntoView({ behavior: 'smooth' });
    }
  }, [messages.length, conversationId]);

  return (
    <div className="flex flex-col h-full bg-zinc-950 relative">
      <div className="absolute inset-0 bg-[radial-gradient(circle_at_top,_var(--tw-gradient-stops))] from-zinc-900/20 via-zinc-950 to-zinc-950 pointer-events-none"></div>
      
      <header className="px-6 py-4 bg-zinc-950/80 border-b border-zinc-800/50 backdrop-blur-xl sticky top-0 z-10 flex items-center justify-between">
        <div className="flex items-center gap-4">
            <div className="w-10 h-10 rounded bg-zinc-900 border border-zinc-800 flex items-center justify-center text-zinc-400 shadow-sm">
                <Bot size={20} />
            </div>
            <div>
                <h3 className="text-zinc-100 font-semibold text-sm leading-tight flex items-center gap-2">
                   {conversation?.title || 'Loading Session...'}
                </h3>
                <div className="flex items-center gap-2 mt-1">
                  <span className="text-[10px] text-zinc-500 font-mono flex items-center gap-1 bg-zinc-900 px-1.5 py-0.5 rounded border border-zinc-800/50">
                    <Hash size={8} /> {conversationId.split('-')[0]}
                  </span>
                  {conversation?.configuration?.modelName && (
                     <span className="text-[10px] text-zinc-500 font-mono px-1.5 py-0.5 rounded border border-zinc-800/50">
                       {conversation.configuration.modelName}
                     </span>
                  )}
                </div>
            </div>
        </div>
        <button className="p-2 text-zinc-600 hover:text-zinc-300 transition-colors">
          <Info size={18} />
        </button>
      </header>

      <div 
        ref={scrollRef} 
        className="flex-1 overflow-y-auto p-4 md:p-8 space-y-10 scroll-smooth"
      >
        {isMessagesLoading ? (
          <div className="flex flex-col items-center justify-center h-full text-zinc-500 gap-3">
            <Loader2 className="animate-spin text-zinc-400" size={24} />
            <p className="text-xs font-mono animate-pulse">SYNCING_HISTORY...</p>
          </div>
        ) : isMessagesError ? (
          <div className="flex flex-col items-center justify-center h-full text-red-400 gap-4">
            <div className="p-4 rounded-full bg-red-500/10 border border-red-500/20">
                <Bot size={32} />
            </div>
            <p className="text-sm font-mono">CONNECTION_INTERRUPTED</p>
            <button 
                onClick={() => refetchMessages()} 
                className="px-4 py-2 bg-zinc-900 hover:bg-zinc-800 rounded border border-zinc-800 text-xs text-zinc-300 transition-colors"
            >
                RETRY_CONNECTION
            </button>
          </div>
        ) : messages.length === 0 ? (
          <div className="flex flex-col items-center justify-center h-full max-w-xl mx-auto text-center px-4">
            <div className="w-16 h-16 rounded-2xl bg-zinc-900 flex items-center justify-center mb-8 border border-zinc-800 shadow-2xl">
                <Command size={32} className="text-zinc-100" />
            </div>
            <h2 className="text-2xl font-bold text-zinc-100 mb-3 tracking-tight">System Ready</h2>
            <p className="text-zinc-500 text-sm mb-10 leading-relaxed max-w-sm">
              Initialize interaction with the neural interface.
            </p>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-3 w-full">
                {[
                  { label: "Refactor Code", desc: "Optimize existing algorithms" },
                  { label: "System Design", desc: "Architect scalable solutions" },
                  { label: "Debug Analysis", desc: "Identify execution errors" },
                  { label: "Documentation", desc: "Generate technical specs" }
                ].map((item) => (
                    <button key={item.label} className="p-4 text-left bg-zinc-900/50 hover:bg-zinc-900 border border-zinc-800 hover:border-zinc-700 rounded-lg group transition-all">
                        <div className="text-zinc-300 text-sm font-medium mb-1 group-hover:text-white">{item.label}</div>
                        <div className="text-zinc-600 text-xs">{item.desc}</div>
                    </button>
                ))}
            </div>
          </div>
        ) : (
          <>
            {messages.map((msg) => (
              <MessageBubble 
                key={msg.id} 
                role={msg.role} 
                content={msg.content} 
                timestamp={msg.createdAt} 
              />
            ))}
            <div ref={bottomRef} className="h-4" />
          </>
        )}
      </div>

      <InputArea conversationId={conversationId} />
    </div>
  );
};

export default ChatArea;