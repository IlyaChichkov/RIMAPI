---
title: RIMAPI
description: RIMAPIは、RimWorldにREST APIサーバーを追加する強力なMODです。
---

# RIMAPI - RimWorld REST API

![RIMAPI Logo](https://raw.githubusercontent.com/IlyaChichkov/RIMAPI/8cdebce963f69c2aeeb50676ab4b994309a1835b/About/preview.png)

[はじめに](quick_start.md){ .md-button .md-button--primary }
[貢献する](contributors_guide/contribute.md){ .md-button }

## 概要

RIMAPIは、RimWorldにREST APIサーバーを追加し、プログラムを通じてゲームと対話できるようにする強力なMODです。コロニーのステータス監視、ポーン（入植者）の制御、リソースの管理、外部ツールやインテグレーションの構築が可能になります。

### 主な機能

- **📡 RESTful API** - ゲームデータ用の標準的なHTTPエンドポイント
- **🔔 リアルタイムイベント** - ライブ更新用のServer-Sent Events (SSE)
- **🔌 拡張可能なアーキテクチャ** - 他のMODが独自のエンドポイントを追加可能
- **🛡️ 安全でノンブロッキング** - リクエストキューイングによりメインスレッドで安全に動作
- **📚 自動ドキュメント生成** - 複数のフォーマットに対応した自己文書化API

### 次のステップ

- [APIドキュメント](https://ilyachichkov.github.io/RIMAPI/api.html)
- [LLMフォーマット](https://ilyachichkov.github.io/RIMAPI/llms-full.txt)