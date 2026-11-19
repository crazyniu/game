/** The bird that stands in each species slot of each world. World 1 is the nine species of the game and the crow; its ids are the species ids, so every save key stays valid.
 *  A bird's ability and when it is met come from its slot species (the core does not know worlds); a world only changes how the slot bird looks. */
export class Worlds {
  static readonly COUNT = 10;
  /** Worlds up to this one have all their birds painted and named; later worlds meet no new birds yet. */
  static readonly READY = 2;
  static readonly SLOTS = ['sunny', 'sky', 'berry', 'mint', 'cocoa', 'snow', 'ember', 'ink', 'hoot'];
  static readonly NAMES = ['green', 'bamboo', 'autumn', 'snow', 'blossom', 'mushroom', 'lakeside', 'pine', 'rainforest', 'wisteria'];
  private static readonly BIRDS: string[][] = [
    ['sunny', 'sky', 'berry', 'mint', 'cocoa', 'snow', 'ember', 'ink', 'hoot'],
    ['oriole', 'magpie', 'hoopoe', 'whiteeye', 'laughingthrush', 'longtailedtit', 'pheasant', 'scopsowl', 'woodowl'],
    ['greattit', 'bluetit', 'bullfinch', 'greenwoodpecker', 'wren', 'wagtail', 'robin', 'nightjar', 'barnowl'],
    ['emperorpenguin', 'bluepenguin', 'gull', 'puffin', 'waxwing', 'snowbunting', 'ptarmigan', 'snowyowl', 'shortearedowl'],
    ['canary', 'bluebird', 'galah', 'hummingbird', 'sparrow', 'javasparrow', 'cardinal', 'nightingale', 'screechowl'],
    ['goldcrest', 'flycatcher', 'jay', 'greenmagpie', 'treecreeper', 'whitecrested', 'scarletfinch', 'frogmouth', 'orientalscops'],
    ['wagtailyellow', 'kingfisher', 'flamingo', 'mallard', 'grebe', 'egret', 'mandarin', 'nightheron', 'fishowl'],
    ['siskin', 'bluejay', 'grosbeak', 'greentit', 'nutcracker', 'spottedwoodpecker', 'crossbill', 'longearedowl', 'borealowl'],
    ['conure', 'bluegoldmacaw', 'spoonbill', 'lovebird', 'toucan', 'cockatoo', 'scarletmacaw', 'potoo', 'spectacledowl'],
    ['narcissus', 'blueflycatcher', 'ibis', 'greenpheasant', 'warbler', 'craneChick', 'redstart', 'hawkowl', 'eagleowl'],
  ];
  /** The bird of a world's slot species, for example bird(2, 'sunny') is 'oriole'. */
  static bird(world: number, slot: string): string { return this.BIRDS[world - 1][this.SLOTS.indexOf(slot)]; }
  /** The bird shown for a board species in a world; the crow looks the same everywhere. */
  static look(world: number, species: string): string { return this.SLOTS.includes(species) ? this.bird(world, species) : species; }
  static has(id: string): boolean { return this.BIRDS.some(row => row.includes(id)); }
  /** The birds of a world in slot order. */
  static birds(world: number): string[] { return this.BIRDS[world - 1]; }
  static slot_of(bird: string): string { for (const row of this.BIRDS) { const i = row.indexOf(bird); if (i >= 0) return this.SLOTS[i]; } return bird; }
  static world_of(bird: string): number { return this.BIRDS.findIndex(row => row.includes(bird)) + 1 || 1; }
}
