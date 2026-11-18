import type { Profile } from './Profile';
import { Progress } from './Progress';
import { Aviary } from './Aviary';
export interface ShopItem { id: string; kind: number; price: number; quantity: number; booster?: number; species?: string; style?: string; index?: number }
export class Shop {
  static readonly BOOSTER = 0;
  static readonly OUTFIT = 1;
  static readonly HOUSE = 2;
  static readonly HOUSE_STYLES = ['mushroom', 'teapot', 'lantern', 'acorn', 'pumpkin', 'beehive', 'teacup', 'pinecone', 'cage'];
  static readonly OUTFIT_PRICES = [300, 500, 800, 1000, 1300];
  static readonly OUTFIT_COUNT = 5;
  private static items: ShopItem[] = [];
  static catalog(): ShopItem[] {
    if (!this.items.length) {
      for (const [kind, id, , price] of Progress.BOOSTERS) {
        this.items.push({ id: 'booster.' + id, kind: this.BOOSTER, price, quantity: 1, booster: kind });
        this.items.push({ id: 'booster.' + id + '.x3', kind: this.BOOSTER, price: Math.floor((price * 5 + 1) / 2), quantity: 3, booster: kind });
      }
      for (const [species] of Aviary.SPECIES) if (species !== 'crow') for (let index = 1; index <= this.OUTFIT_COUNT; index++) this.items.push({ id: `outfit.${species}.${index}`, kind: this.OUTFIT, price: this.OUTFIT_PRICES[index - 1], quantity: 1, species, index });
      for (const style of this.HOUSE_STYLES) this.items.push({ id: 'house.' + style, kind: this.HOUSE, price: 600, quantity: 1, style });
    }
    return this.items;
  }
  static item(id: string): ShopItem | undefined { return this.catalog().find(item => item.id === id); }
  static purchase(p: Profile, id: string, quantity = 1): string {
    const item = this.item(id);
    if (!item) return 'unknown';
    if (item.kind === this.BOOSTER && !Progress.is_booster_unlocked(p, item.booster!)) return 'locked';
    if ((item.kind === this.OUTFIT && p.owns_outfit(id)) || (item.kind === this.HOUSE && p.owns_house(item.style!))) return 'owned';
    if (!Number.isInteger(quantity) || quantity < 1 || (item.kind !== this.BOOSTER && quantity !== 1)) return 'quantity';
    const total = item.price * quantity;
    if (p.coins < total) return 'coins';
    p.coins -= total;
    if (item.kind === this.BOOSTER) p.inventory[item.booster!] += item.quantity * quantity;
    else if (item.kind === this.OUTFIT) p.outfits[id] = true;
    else p.houses[item.style!] = true;
    return '';
  }
  static equip(p: Profile, id: string): string {
    if (id === 'house.default') { p.equipped_house = 'default'; return ''; }
    if (id.startsWith('outfit.') && id.endsWith('.default')) {
      const species = id.slice(7, -8);
      if (!Aviary.has_species(species) || species === 'crow') return 'unknown';
      delete p.equipped[species]; return '';
    }
    const item = this.item(id);
    if (!item) return 'unknown';
    if (item.kind === this.BOOSTER) return 'not_equippable';
    if (item.kind === this.OUTFIT) { if (!p.owns_outfit(id)) return 'not_owned'; p.equipped[item.species!] = id; }
    else { if (!p.owns_house(item.style!)) return 'not_owned'; p.equipped_house = item.style!; }
    return '';
  }
}
