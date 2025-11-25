import React, { useState } from 'react';
import { usePostApiV1ConversationsIdMessages } from '../openapi/generated/api';
import { useQueryClient } from '@tanstack/react-query';
import { Paperclip, Loader2, ArrowUp } from 'lucide-react';

interface InputAreaProps {
  conversationId: string;
}

const InputArea: React.FC<InputAreaProps> = ({ conversationId }) => {
  const [content, setContent] = useState('');
  const queryClient = useQueryClient();

  const { mutate, isPending } = usePostApiV1ConversationsIdMessages({
    mutation: {
      onSuccess: () => {
        setContent('');
        queryClient.invalidateQueries({ 
          queryKey: [`/conversations/${conversationId}/messages`] 
        });
      },
      onError: (err: unknown) => {
        console.error("Failed to send", err);
      }
    }
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!content.trim() || isPending) return;

    mutate({
      id: conversationId,
      data: {
        content: content,
        attachments: []
      }
    });
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSubmit(e as unknown as React.FormEvent);
    }
  };

  return (
    <div className="p-4 md:p-6 bg-gradient-to-t from-zinc-950 via-zinc-950/95 to-transparent sticky bottom-0 z-20">
      <div className="max-w-4xl mx-auto relative">
        <form 
          onSubmit={handleSubmit}
          className="relative flex items-end gap-2 bg-zinc-900/50 backdrop-blur-xl rounded-xl p-2 border border-zinc-700/50 shadow-2xl focus-within:border-zinc-500/50 focus-within:ring-1 focus-within:ring-zinc-500/20 transition-all duration-300"
        >
          <button
            type="button"
            className="p-3 text-zinc-500 hover:text-zinc-300 hover:bg-zinc-800 rounded-lg transition-colors mb-0.5"
            title="Attach Context"
          >
            <Paperclip size={18} />
          </button>
          
          <textarea
            value={content}
            onChange={(e) => setContent(e.target.value)}
            onKeyDown={handleKeyDown}
            placeholder="Enter instructions..."
            className="flex-1 max-h-48 min-h-[52px] py-3.5 px-2 bg-transparent text-zinc-100 placeholder-zinc-500 focus:outline-none resize-none overflow-y-auto custom-scrollbar text-[15px]"
            rows={1}
            style={{ height: 'auto' }}
            onInput={(e) => {
                const target = e.target as HTMLTextAreaElement;
                target.style.height = 'auto';
                target.style.height = `${Math.min(target.scrollHeight, 192)}px`;
            }}
          />

          <button
            type="submit"
            disabled={!content.trim() || isPending}
            className={`p-3 rounded-lg mb-0.5 transition-all duration-200 flex items-center justify-center ${
              content.trim() && !isPending
                ? 'bg-zinc-100 text-zinc-950 hover:bg-white shadow-lg shadow-white/10' 
                : 'bg-zinc-800 text-zinc-600 cursor-not-allowed'
            }`}
          >
            {isPending ? <Loader2 size={18} className="animate-spin" /> : <ArrowUp size={18} strokeWidth={2.5} />}
          </button>
        </form>
        <div className="text-center mt-3 text-[10px] text-zinc-600 font-mono">
           AI generated responses may vary.
        </div>
      </div>
    </div>
  );
};

export default InputArea;